// book/samples/Chapter-01/DistributedPaymentService/tests/Integration/PaymentProcessorIntegrationTests.cs
//
// S02 concept: tests that reveal the failure modes invisible in unit tests.
// These tests address the production failures described in code-01-02:
// idempotency edge cases and gateway-timeout typing.
//
// Honesty note: these doubles are IN-MEMORY, not real infrastructure.
// No Redis runs here; TestPaymentRepository is a Dictionary, not a test DB.
// What they genuinely verify is processor logic against shaped fakes —
// the S02 lesson (test the failure mode, not just the specification)
// without claiming production fidelity they do not have.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaymentService.Application.Ports;
using PaymentService.Application.UseCases;
using PaymentService.Core.Domain;

namespace PaymentService.Tests.Integration;

/// <summary>
/// Integration tests that verify behaviour under the production failure modes
/// described in Chapter 01, Section 02.
///
/// These complement the unit tests, which verify specification correctness.
/// These verify behavioural correctness under real operational conditions.
/// </summary>
[TestClass]
public sealed class PaymentProcessorIntegrationTests
{
    // ── Test 1: Idempotency survives process restart ──────────────────────
    // Addresses S01 failure: cache-based idempotency fails on restart.
    // The corrected processor writes idempotency key to the durable store.

    [TestMethod]
    public async Task ProcessAsync_DurableIdempotency_SurvivesProcessRestart()
    {
        // Arrange: in-memory repository shaped like the durable store
        // (same interface the processor uses in production).
        var repository = new TestPaymentRepository();
        var gateway    = new AlwaysSuccessGateway();
        var processor  = new PaymentProcessor(gateway, repository, NullLogger.Instance);

        var request = new ProcessPaymentRequest(
            IdempotencyKey: "idem-key-restart-test",
            CustomerId:     CustomerId.New(),
            Amount:         Money.Of(100m),
            Method:         PaymentMethod.Card("tok_test"));

        // Act 1: First call — should succeed.
        var result1 = await processor.ProcessAsync(request, default);
        Assert.IsTrue(result1.IsSuccess);

        // Simulate process restart: create a NEW processor instance.
        // If idempotency lived in memory or cache, this new instance wouldn't know.
        var processor2 = new PaymentProcessor(gateway, repository, NullLogger.Instance);

        // Act 2: Same request to new instance — must return Duplicate.
        var result2 = await processor2.ProcessAsync(request, default);
        Assert.IsTrue(result2.IsDuplicate);

        // Gateway must have been called exactly once.
        Assert.AreEqual(1, gateway.ChargeCallCount);
    }

    // ── Test 2: Gateway timeout returns 503, not 500 ─────────────────────
    // Addresses S03 architectural decision: typed failure response.

    [TestMethod]
    public async Task ProcessAsync_GatewayTimeout_ReturnsGatewayTimeoutResult()
    {
        var repository = new TestPaymentRepository();
        // Gateway that hangs for 10 seconds — longer than our 4s timeout.
        var gateway    = new SlowGateway(delay: TimeSpan.FromSeconds(10));
        var processor  = new PaymentProcessor(gateway, repository, NullLogger.Instance);

        var request = new ProcessPaymentRequest(
            IdempotencyKey: "idem-key-timeout-test",
            CustomerId:     CustomerId.New(),
            Amount:         Money.Of(50m),
            Method:         PaymentMethod.Card("tok_test"));

        // Act: call with a cancellation token that outlasts the gateway timeout.
        var result = await processor.ProcessAsync(request, default);

        // Assert: GatewayTimeout result, not an exception.
        Assert.AreEqual("GATEWAY_TIMEOUT", result.ErrorCode);
        // Payment should be persisted as Failed (not lost).
        Assert.IsTrue(repository.HasFailedPayment);
    }

    // ── Test 3: Idempotency key recorded atomically with payment ─────────
    // Verifies the S01 fix: a single repository call persists both, so no
    // crash window exists between the payment write and the key write.

    [TestMethod]
    public async Task ProcessAsync_AtomicWrite_IdempotencyKeyAndPaymentBothPersisted()
    {
        var repository = new TestPaymentRepository();
        var gateway    = new AlwaysSuccessGateway();
        var processor  = new PaymentProcessor(gateway, repository, NullLogger.Instance);

        var key     = "idem-key-atomic-test";
        var request = new ProcessPaymentRequest(
            IdempotencyKey: key,
            CustomerId:     CustomerId.New(),
            Amount:         Money.Of(200m),
            Method:         PaymentMethod.Card("tok_atomic"));

        await processor.ProcessAsync(request, default);

        // Both the payment and the idempotency key must exist.
        Assert.IsTrue(repository.HasPayment);
        Assert.IsTrue(await repository.IdempotencyKeyExistsAsync(key, default));
    }

    // ── Test 4: Unreachable gateway returns 503, not 500 ────────────────
    // A dependency failure (DNS, refused connection) is the provider's
    // problem. The processor types it like a timeout: retryable 503,
    // payment persisted as Failed.

    [TestMethod]
    public async Task ProcessAsync_GatewayUnreachable_ReturnsGatewayUnreachableResult()
    {
        var repository = new TestPaymentRepository();
        var gateway    = new UnreachableGateway();
        var processor  = new PaymentProcessor(gateway, repository, NullLogger.Instance);

        var request = new ProcessPaymentRequest(
            IdempotencyKey: "idem-key-unreachable-test",
            CustomerId:     CustomerId.New(),
            Amount:         Money.Of(60m),
            Method:         PaymentMethod.Card("tok_test"));

        var result = await processor.ProcessAsync(request, default);

        Assert.AreEqual("GATEWAY_UNREACHABLE", result.ErrorCode);
        Assert.IsTrue(repository.HasFailedPayment);
    }

    // ── Test doubles ─────────────────────────────────────────────────────

    private sealed class AlwaysSuccessGateway : IPaymentGateway
    {
        public int ChargeCallCount { get; private set; }

        public Task<GatewayResponse> ChargeAsync(
            Money amount, PaymentMethod method, CancellationToken ct)
        {
            ChargeCallCount++;
            return Task.FromResult(
                new GatewayResponse(true, $"tx_{ChargeCallCount}", null));
        }
    }

    private sealed class SlowGateway(TimeSpan delay) : IPaymentGateway
    {
        public async Task<GatewayResponse> ChargeAsync(
            Money amount, PaymentMethod method, CancellationToken ct)
        {
            await Task.Delay(delay, ct);
            return new GatewayResponse(true, "tx_slow", null);
        }
    }

    private sealed class UnreachableGateway : IPaymentGateway
    {
        public Task<GatewayResponse> ChargeAsync(
            Money amount, PaymentMethod method, CancellationToken ct) =>
            throw new HttpRequestException("Simulated DNS failure.");
    }

    private sealed class TestPaymentRepository : IPaymentRepository
    {
        private readonly Dictionary<string, PaymentId> _keys = new();
        private readonly List<Payment> _payments = new();
        private readonly object _gate = new();

        public bool HasPayment       => _payments.Count > 0;
        public bool HasFailedPayment => _payments.Any(p =>
            p.Status == PaymentStatus.Failed);

        // Mirrors production atomicity: one lock, both writes, no window.
        public Task SaveAsync(Payment payment, string idempotencyKey, CancellationToken ct)
        {
            lock (_gate)
            {
                _payments.Add(payment);
                _keys[idempotencyKey] = payment.Id;
            }
            return Task.CompletedTask;
        }

        public Task<bool> IdempotencyKeyExistsAsync(string key, CancellationToken ct)
            => Task.FromResult(_keys.ContainsKey(key));

        public Task<Payment?> FindByIdAsync(PaymentId id, CancellationToken ct)
            => Task.FromResult(_payments.FirstOrDefault(p => p.Id == id));
    }

    private static class NullLogger
    {
        public static Microsoft.Extensions.Logging.ILogger<T> Create<T>() =>
            Microsoft.Extensions.Logging.Abstractions.NullLogger<T>.Instance;

        public static Microsoft.Extensions.Logging.ILogger<PaymentProcessor> Instance =>
            Microsoft.Extensions.Logging.Abstractions.NullLogger<PaymentProcessor>.Instance;
    }
}
