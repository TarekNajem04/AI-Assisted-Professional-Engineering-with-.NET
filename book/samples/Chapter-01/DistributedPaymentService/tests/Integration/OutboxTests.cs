// book/samples/Chapter-01/DistributedPaymentService/tests/Integration/OutboxTests.cs
//
// S05 concept: the outbox's at-least-once contract, verified — not assumed.
// Each test maps to one of the three stated obligations: contract (valid
// events only), failure modes (redelivery after crash-before-mark),
// visibility (PendingCount tracks dispatch depth).

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaymentService.Application.Ports;
using PaymentService.Infrastructure.Messaging;

namespace PaymentService.Tests.Integration;

[TestClass]
public sealed class OutboxTests
{
    // ── Obligation 1 (contract): malformed events never enter the stream ──

    [TestMethod]
    public async Task StoreAsync_EmptyEventType_Throws()
    {
        var outbox = new InMemoryOutbox();

        await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            outbox.StoreAsync("", """{"orderId":"o-1"}""", default));
    }

    // ── Happy path: store → read → dispatch → mark ────────────────────────

    [TestMethod]
    public async Task Store_Dispatch_Mark_RemovesFromPending()
    {
        var outbox = new InMemoryOutbox();

        var stored = await outbox.StoreAsync(
            "OrderPlaced", """{"orderId":"o-1"}""", default);
        Assert.AreEqual(1, outbox.PendingCount);

        var pending = await outbox.ReadPendingAsync(10, default);
        Assert.AreEqual(1, pending.Count);
        Assert.AreEqual(stored.Sequence, pending[0].Sequence);
        Assert.AreEqual("OrderPlaced", pending[0].EventType);

        await outbox.MarkDispatchedAsync(stored.Sequence, default);
        Assert.AreEqual(0, outbox.PendingCount);
    }

    // ── Obligation 2 (failure modes): crash-before-mark redelivers ────────
    // Simulates a dispatcher that delivered the event but crashed before
    // marking it. The event MUST reappear: at-least-once, duplicates to the
    // (idempotent) consumer rather than a lost event.

    [TestMethod]
    public async Task CrashBeforeMark_EventIsRedelivered()
    {
        var outbox = new InMemoryOutbox();

        var stored = await outbox.StoreAsync(
            "OrderPlaced", """{"orderId":"o-2"}""", default);

        // Dispatcher delivers… then crashes before MarkDispatchedAsync.
        var firstRead = await outbox.ReadPendingAsync(10, default);
        Assert.AreEqual(1, firstRead.Count);

        // Restarted dispatcher reads again: the event is still there.
        var secondRead = await outbox.ReadPendingAsync(10, default);
        Assert.AreEqual(1, secondRead.Count);
        Assert.AreEqual(stored.Sequence, secondRead[0].Sequence);
    }

    // ── Obligation 3 (visibility): depth is observable ────────────────────

    [TestMethod]
    public async Task PendingCount_TracksDispatchDepth()
    {
        var outbox = new InMemoryOutbox();

        Assert.AreEqual(0, outbox.PendingCount);

        var first = await outbox.StoreAsync("A", """{"n":1}""", default);
        await outbox.StoreAsync("B", """{"n":2}""", default);
        Assert.AreEqual(2, outbox.PendingCount);

        await outbox.MarkDispatchedAsync(first.Sequence, default);
        Assert.AreEqual(1, outbox.PendingCount);
    }

    // ── Adapter honesty: concurrent stores stay atomic and ordered ────────

    [TestMethod]
    public async Task ConcurrentStores_SequencesStayUnique()
    {
        var outbox = new InMemoryOutbox();

        var stored = await Task.WhenAll(Enumerable.Range(0, 50).Select(i =>
            outbox.StoreAsync("E", $$"""{"n":{{i}}}""", default)));

        Assert.AreEqual(50, stored.Select(m => m.Sequence).Distinct().Count());
        Assert.AreEqual(50, outbox.PendingCount);
    }
}
