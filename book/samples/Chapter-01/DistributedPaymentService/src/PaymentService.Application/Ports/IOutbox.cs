// book/samples/Chapter-01/DistributedPaymentService/src/PaymentService.Application/Ports/IOutbox.cs
//
// S05 concept: the responsibility boundary for reliable event publication.
//
// The outbox pattern exists because publishing an event and recording the
// business fact are two writes that must succeed or fail together.
// The three verification obligations from S05, stated for this contract:
//
//   1. CONTRACT: every stored event carries EventType + Payload schema the
//      consumer was built against. Changing either without updating the
//      consumer breaks a behavioural contract no interface signature shows.
//   2. FAILURE MODES: delivery is at-least-once. A crash between dispatch
//      and MarkDispatchedAsync redelivers the event. Consumers MUST be
//      idempotent — that requirement lives with the consumer, not here.
//   3. VISIBILITY: PendingCount exposes dispatch-queue depth. A growing
//      depth is the observable signal of a stalled dispatcher.
//
namespace PaymentService.Application.Ports;

public sealed record OutboxMessage(
    long Sequence,
    string EventType,
    string Payload,
    DateTimeOffset StoredAt);

public interface IOutbox
{
    /// <summary>
    /// Appends one event to the dispatch stream. The single append is
    /// indivisible (sequence assignment and insert happen as one unit), so
    /// callers never observe a half-stored event. What this call does NOT do
    /// is bundle an external business write: in production the caller issues
    /// this store inside the SAME database transaction as its business row;
    /// that pairing is the caller's responsibility under this contract.
    /// After this returns, the event WILL eventually be dispatched
    /// (at-least-once) — provided the process (or its durable successor)
    /// keeps a dispatcher running.
    /// </summary>
    Task<OutboxMessage> StoreAsync(
        string eventType, string payload, CancellationToken ct);

    /// <summary>
    /// Returns up to <paramref name="maxCount"/> undispatched events,
    /// oldest first. Events remain pending until marked dispatched, so a
    /// dispatcher that crashes mid-batch redelivers on restart.
    /// </summary>
    Task<IReadOnlyList<OutboxMessage>> ReadPendingAsync(
        int maxCount, CancellationToken ct);

    /// <summary>
    /// Marks an event dispatched. Only call after the consumer has
    /// durably accepted the event — marking early loses events on crash.
    /// </summary>
    Task MarkDispatchedAsync(long sequence, CancellationToken ct);

    /// <summary>
    /// Operational visibility: number of events awaiting dispatch.
    /// Alert when this grows monotonically — the dispatcher is stalled.
    /// </summary>
    int PendingCount { get; }
}
