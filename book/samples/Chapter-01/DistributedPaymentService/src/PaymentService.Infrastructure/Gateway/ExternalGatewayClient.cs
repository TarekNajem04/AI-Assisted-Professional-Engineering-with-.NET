// book/samples/Chapter-01/DistributedPaymentService/src/PaymentService.Infrastructure/Gateway/ExternalGatewayClient.cs
//
// S04 concept: AI-assisted Polly resilience pipeline, engineer-adjusted.
//
// The AI generated a correct general-pattern Polly configuration.
// Before deploying, the engineer answered the architecture-centric questions:
//   Q: "What is the actual traffic pattern?"          → ~3 req/min avg
//   Q: "What is this service's own timeout SLA?"       → 5 seconds
//   Q: "Does the gateway return 429 (rate-limit)?"     → yes, above 200/hour
//   Q: "What is acceptable blast radius on failure?"   → callers receive 503
//
// Each parameter below reflects those answers, not the AI default.

using System.Net;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using PaymentService.Application.Ports;
using PaymentService.Core.Domain;
using Polly;

namespace PaymentService.Infrastructure.Gateway;

public static class ExternalGatewayClientRegistration
{
    public static IHttpClientBuilder AddExternalGatewayClient(
        this IServiceCollection services)
    {
        var builder = services
            .AddHttpClient<IPaymentGateway, ExternalGatewayClient>();

        builder.AddResilienceHandler("payment-gateway", pipeline =>
        {
            pipeline
                // Retry: fewer attempts for low-traffic service.
                // 429 added because this gateway rate-limits.
                .AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 2,           // reduced from AI default of 3
                    Delay            = TimeSpan.FromMilliseconds(500),
                    BackoffType      = DelayBackoffType.Exponential,
                    UseJitter        = true,
                    ShouldHandle     = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .HandleResult(r =>
                            r.StatusCode >= HttpStatusCode.InternalServerError ||
                            r.StatusCode == HttpStatusCode.TooManyRequests) // 429
                })

                // Circuit Breaker: thresholds calibrated to ~3 req/min.
                // MinimumThroughput = 3 (not 10) so breaker can trip at low traffic.
                // SamplingDuration = 60s (not 30s) to accumulate enough samples.
                .AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio      = 0.5,
                    SamplingDuration  = TimeSpan.FromSeconds(60),
                    MinimumThroughput = 3,           // reflects actual traffic
                    BreakDuration     = TimeSpan.FromSeconds(30),
                })

                // Timeout: below this service's 5s SLA.
                // Ensures gateway timeout fires before caller's budget expires.
                .AddTimeout(TimeSpan.FromSeconds(3.5)); // below 5s SLA
        });

        return builder;
    }
}

public sealed class ExternalGatewayClient(
    HttpClient http,
    ILogger<ExternalGatewayClient> logger) : IPaymentGateway
{
    public async Task<GatewayResponse> ChargeAsync(
        Money amount, PaymentMethod method, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(method);

        var body = new
        {
            amount   = amount.Value,
            currency = amount.Currency,
            source   = method.Token,
            capture  = true,
        };

        using var response = await http.PostAsJsonAsync("/v1/charges", body, ct);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content
                .ReadFromJsonAsync<GatewaySuccessDto>(ct);
            logger.LogInformation(
                "Gateway charged {Amount} {Currency} → {TxId}",
                amount.Value, amount.Currency, result?.Id);
            return new GatewayResponse(true, result?.Id ?? "", null);
        }

        var error = await response.Content
            .ReadFromJsonAsync<GatewayErrorDto>(ct);
        logger.LogWarning(
            "Gateway declined: {Code}", error?.Code);
        return new GatewayResponse(false, "", error?.Code);
    }

    private sealed record GatewaySuccessDto(string Id);
    private sealed record GatewayErrorDto(string Code, string Message);
}
