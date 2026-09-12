// book/samples/Chapter-01/DistributedPaymentService/src/PaymentService.Infrastructure/Messaging/InMemoryMessageBus.cs
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using PaymentService.Application.Ports;

namespace PaymentService.Infrastructure.Messaging;

public sealed class InMemoryMessageBus : IMessageBus
{
    private readonly ConcurrentQueue<Message> _queue = new();
    // Acknowledgement set must be thread-safe: the worker acknowledges while
    // monitors probe. A plain HashSet<string> raced here — ConcurrentDictionary
    // used as a set closes it.
    private readonly ConcurrentDictionary<string, byte> _acked = new();
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(200);

    public async IAsyncEnumerable<MessageBatch> ReadBatchesAsync([EnumeratorCancellation] CancellationToken ct)
    {
        int batchSequence = 0;
        while (!ct.IsCancellationRequested)
        {
            var messages = new List<Message>();
            while (messages.Count < 10 && _queue.TryDequeue(out var msg))
                messages.Add(msg);

            if (messages.Count == 0)
            {
                await Task.Delay(PollInterval, ct).ConfigureAwait(false);
                continue;
            }

            yield return new MessageBatch($"batch-{++batchSequence:D4}", messages);
        }
    }

    public Task AcknowledgeAsync(string batchId, CancellationToken ct)
    {
        _acked.TryAdd(batchId, 0);
        return Task.CompletedTask;
    }

    public Task PublishAsync(Message message, CancellationToken ct)
    {
        _queue.Enqueue(message);
        return Task.CompletedTask;
    }

    public void Enqueue(string id, string payload, string topic = "default")
        => _queue.Enqueue(new Message(id, payload, topic));

    public bool IsAcknowledged(string batchId) => _acked.ContainsKey(batchId);
    public int PendingCount => _queue.Count;
}
