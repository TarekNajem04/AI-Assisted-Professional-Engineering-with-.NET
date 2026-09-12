// book/samples/Chapter-01/DistributedPaymentService/src/PaymentService.Infrastructure/Messaging/InMemoryOutbox.cs
//
// S05 concept: responsibility boundary for reliable event publication.
//
// This adapter stands in for a transactional outbox table. The production
// version would append the event row in the SAME database transaction as
// the business write; here a single lock makes the pair indivisible.
// The honesty note: this lock survives threads, not process restarts.
// A durable outbox survives restarts — that is the documented gap between
// this adapter and production, and the reason the dispatcher below treats
// every pending event as redeliverable.
//
// The three S05 obligations, discharged here:
//
//   1. CONTRACT: StoreAsync rejects empty event types and payloads, so a
//      malformed event can never enter the dispatch stream.
//   2. FAILURE MODES: at-least-once delivery. ReadPendingAsync returns
//      undispatched events on every call — including events a crashed
//      dispatcher already delivered but never marked. Duplicates are the
//      consumer's responsibility (idempotent consumers only).
//   3. VISIBILITY: PendingCount is lock-observed dispatch depth. A monitor
//      that samples it sees dispatcher stalls as a rising number.
//
using PaymentService.Application.Ports;

namespace PaymentService.Infrastructure.Messaging;

public sealed class InMemoryOutbox : IOutbox
{
    private readonly object _gate = new();
    private readonly Dictionary<long, OutboxMessage> _pending = new();
    private long _sequence;

    public int PendingCount
    {
        get { lock (_gate) { return _pending.Count; } }
    }

    public Task<OutboxMessage> StoreAsync(
        string eventType, string payload, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        lock (_gate)
        {
            var message = new OutboxMessage(
                Sequence: ++_sequence,
                EventType: eventType,
                Payload: payload,
                StoredAt: DateTimeOffset.UtcNow);
            _pending.Add(message.Sequence, message);
            return Task.FromResult(message);
        }
    }

    public Task<IReadOnlyList<OutboxMessage>> ReadPendingAsync(
        int maxCount, CancellationToken ct)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCount);

        lock (_gate)
        {
            IReadOnlyList<OutboxMessage> batch = _pending.Values
                .OrderBy(m => m.Sequence)
                .Take(maxCount)
                .ToArray();   // snapshot under the lock; dispatch outside it
            return Task.FromResult(batch);
        }
    }

    public Task MarkDispatchedAsync(long sequence, CancellationToken ct)
    {
        lock (_gate)
        {
            _pending.Remove(sequence);
        }
        return Task.CompletedTask;
    }
}
