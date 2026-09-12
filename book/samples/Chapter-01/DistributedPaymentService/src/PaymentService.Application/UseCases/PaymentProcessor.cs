// book/samples/Chapter-01/DistributedPaymentService/src/PaymentService.Application/UseCases/PaymentProcessor.cs
//
// This is the corrected version of the PaymentProcessor from S01.
// Every assumption that was implicit in the original is now explicit.
// Each architectural decision is documented with its engineering reasoning.

using Microsoft.Extensions.Logging;
using PaymentService.Application.Ports;
using PaymentService.Core.Domain;

namespace PaymentService.Application.UseCases;

/// <summary>
/// Processes a payment request with explicit handling of every failure mode
/// identified in Chapter 01, Section 01.
///
/// Architecture decisions documented (S03 practice):
///
/// 1. IDEMPOTENCY: the idempotency check reads a durable store (DB), not a
///    cache, and the payment record plus idempotency key are written in ONE
///    atomic repository call. This closes the S01 crash-between-writes hole,
///    where a crash after the DB write but before the cache write caused the
///    retry to miss and double-charge. Concurrent duplicates remain guarded
///    by a production unique constraint (see IPaymentRepository contract).
///
/// 2. TIMEOUT BUDGET: The gateway call has an explicit 4-second timeout,
///    below this service's own 5-second SLA. Caller's overall budget is 8s.
///    This leaves headroom for the DB write and response serialization.
///
/// 3. CANCELLATION: CancellationToken is propagated to every async call.
///    The gateway call uses a linked token so both caller cancellation and
///    timeout cancellation work correctly without orphaning the connection.
/// </summary>
public sealed class PaymentProcessor(
    IPaymentGateway gateway,
    IPaymentRepository repository,
    ILogger<PaymentProcessor> logger)
{
    // Gateway timeout is below this service's own timeout (5s).
    // Explicit, not default. Changing this requires understanding the budget.
    private static readonly TimeSpan GatewayTimeout = TimeSpan.FromSeconds(4);

    public async Task<ProcessPaymentResult> ProcessAsync(
        ProcessPaymentRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        // ── Idempotency check ─────────────────────────────────────────────
        // Architecture note: This checks a durable store (DB), not a cache.
        // Cache-based idempotency (original S01 code) fails on process restart
        // because the cache entry may not exist after the DB write succeeded.
        if (await repository.IdempotencyKeyExistsAsync(request.IdempotencyKey, ct))
        {
            logger.LogInformation(
                "Duplicate payment request {Key} — returning early",
                request.IdempotencyKey);
            return ProcessPaymentResult.Duplicate;
        }

        // ── Domain object creation ────────────────────────────────────────
        var payment = Payment.Create(
            request.CustomerId,
            request.Amount,
            request.Method);

        // ── Gateway call with explicit timeout ────────────────────────────
        // Architecture note: Linked token handles two cancellation sources:
        // (a) caller disconnects → request CT fires
        // (b) gateway takes too long → timeout CT fires
        // Without the link, timeout fires but orphaned gateway connection
        // continues to consume thread pool slots.
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(GatewayTimeout);

        GatewayResponse gatewayResponse;
        try
        {
            gatewayResponse = await gateway.ChargeAsync(
                payment.Amount, payment.Method, timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            // Timeout, not caller cancellation — return typed error, not 500.
            logger.LogWarning(
                "Gateway timeout for payment {PaymentId}", payment.Id);
            payment.MarkFailed();
            await repository.SaveAsync(payment, request.IdempotencyKey, ct);
            return ProcessPaymentResult.GatewayTimeout;
        }
        catch (HttpRequestException ex)
        {
            // Dependency unreachable (DNS, refused, TLS) — the provider's
            // problem, not ours. Typed 503 like a timeout: retryable, and
            // never a 500 that would blame this service's code.
            logger.LogWarning(
                ex, "Gateway unreachable for payment {PaymentId}", payment.Id);
            payment.MarkFailed();
            await repository.SaveAsync(payment, request.IdempotencyKey, ct);
            return ProcessPaymentResult.GatewayUnreachable;
        }

        // ── Apply domain transition ───────────────────────────────────────
        if (gatewayResponse.Success)
            payment.MarkProcessed();
        else
            payment.MarkFailed();

        // ── Durable write ─────────────────────────────────────────────────
        // ONE atomic call: payment record + idempotency key together.
        // The adapter guarantees indivisibility (transaction in production,
        // single lock in-memory) — no crash window between the two writes.
        await repository.SaveAsync(payment, request.IdempotencyKey, ct);

        logger.LogInformation(
            "Payment {PaymentId} {Status}", payment.Id, payment.Status);

        return gatewayResponse.Success
            ? ProcessPaymentResult.Success(payment.Id)
            : ProcessPaymentResult.GatewayDeclined(gatewayResponse.ErrorCode);
    }
}

// ── Request / Result types ────────────────────────────────────────────────

public sealed record ProcessPaymentRequest(
    string         IdempotencyKey,
    CustomerId     CustomerId,
    Money          Amount,
    PaymentMethod  Method);

public sealed record ProcessPaymentResult
{
    public bool      IsSuccess    { get; private init; }
    public bool      IsDuplicate  { get; private init; }
    public PaymentId? PaymentId   { get; private init; }
    public string?   ErrorCode    { get; private init; }

    public static ProcessPaymentResult Success(PaymentId id) =>
        new() { IsSuccess = true, PaymentId = id };

    public static readonly ProcessPaymentResult Duplicate =
        new() { IsDuplicate = true };

    public static readonly ProcessPaymentResult GatewayTimeout =
        new() { ErrorCode = "GATEWAY_TIMEOUT" };

    public static readonly ProcessPaymentResult GatewayUnreachable =
        new() { ErrorCode = "GATEWAY_UNREACHABLE" };

    public static ProcessPaymentResult GatewayDeclined(string? code) =>
        new() { ErrorCode = code ?? "DECLINED" };
}
