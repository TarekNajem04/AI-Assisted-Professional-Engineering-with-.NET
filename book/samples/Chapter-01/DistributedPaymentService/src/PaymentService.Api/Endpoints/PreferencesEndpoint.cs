// book/samples/Chapter-01/DistributedPaymentService/src/PaymentService.Api/Endpoints/PreferencesEndpoint.cs
//
// S03 concept: Implementation B from the dual-implementation example.
// Every decision reflects the architecture-centric questions asked before coding.
//
// Questions asked before writing a line:
//   Q: "Latency budget for this endpoint?"           → 500ms total
//   Q: "Redis cold-start scenario?"                  → possible after deploy
//   Q: "Caller contract on cache miss?"              → degraded but not error
//   Q: "Cache stampede risk?"                        → yes at deploy time
//   Q: "CancellationToken propagation correct?"      → enforced below

using Microsoft.Extensions.Caching.Distributed;
using PaymentService.Application.Ports;
using System.Text.Json;

namespace PaymentService.Api.Endpoints;

public static class PreferencesEndpoint
{
    public static void MapPreferencesEndpoints(this WebApplication app)
    {
        app.MapGet("/users/{userId}/preferences",
            async (
                string userId,
                IUserPreferenceService preferenceService,
                IDistributedCache cache,
                ILogger<Program> logger,
                CancellationToken ct) =>
            {
                // ── Cache read — bounded to 50ms ──────────────────────────
                // Redis cold-starts after deployment can reach 150–200ms.
                // Without this bound, they consume the entire 500ms budget.
                // Cache misses AND cache outages fall through silently —
                // caller still gets data from the source of truth.
                // Only caller cancellation propagates; everything else degrades.
                string? cached = null;
                try
                {
                    using var cacheTimeout = CancellationTokenSource
                        .CreateLinkedTokenSource(ct);
                    cacheTimeout.CancelAfter(TimeSpan.FromMilliseconds(50));
                    cached = await cache.GetStringAsync(userId, cacheTimeout.Token);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (OperationCanceledException)
                {
                    // Cache budget exceeded — observable, not silent.
                    logger.LogWarning(
                        "Cache read timeout for user {UserId}", userId);
                }
                catch (Exception ex)
                {
                    // Cache unavailable (connection failure, Redis down) —
                    // degraded but not error. Caller still gets data below.
                    logger.LogWarning(
                        ex, "Cache unavailable for user {UserId}", userId);
                }

                if (cached is not null)
                    return Results.Ok(
                        JsonSerializer.Deserialize<UserPreferences>(cached));

                // ── Source-of-truth read — bounded to 300ms ───────────────
                // Leaves ~150ms for DB write + response within the 500ms budget.
                UserPreferences preferences;
                try
                {
                    using var serviceTimeout = CancellationTokenSource
                        .CreateLinkedTokenSource(ct);
                    serviceTimeout.CancelAfter(TimeSpan.FromMilliseconds(300));
                    preferences = await preferenceService
                        .GetAsync(userId, serviceTimeout.Token);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (OperationCanceledException)
                {
                    logger.LogError(
                        "Preference service timeout for user {UserId}", userId);
                    // 503 tells callers to retry — not a 500 (our bug).
                    return Results.StatusCode(503);
                }

                // ── Write-behind with jittered TTL ────────────────────────
                // (a) Write-behind: don't block the response on cache write.
                //     A failed cache write is a perf degradation, not an error.
                // (b) Jitter (±60s): prevents simultaneous expiry of thousands
                //     of entries 15 minutes after a deployment (cache stampede).
                _ = cache.SetStringAsync(
                    userId,
                    JsonSerializer.Serialize(preferences),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow =
                            TimeSpan.FromMinutes(15) +
                            TimeSpan.FromSeconds(Random.Shared.Next(0, 60))
                    },
                    CancellationToken.None)   // intentional: don't cancel on disconnect
                    .ConfigureAwait(false);

                return Results.Ok(preferences);
            })
            .WithName("GetUserPreferences")
            .Produces<UserPreferences>()
            .Produces(503);
    }
}
