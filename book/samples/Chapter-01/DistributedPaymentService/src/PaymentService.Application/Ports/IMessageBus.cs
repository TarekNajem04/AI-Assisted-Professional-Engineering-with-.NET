// book/samples/Chapter-01/DistributedPaymentService/src/PaymentService.Application/Ports/IMessageBus.cs
namespace PaymentService.Application.Ports;

public sealed record Message(string Id, string Payload, string Topic);
public sealed record MessageBatch(string BatchId, IReadOnlyList<Message> Messages);
public sealed record ProcessingResult(bool Success, string MessageId, string? Error = null);

public interface IMessageBus
{
    IAsyncEnumerable<MessageBatch> ReadBatchesAsync(CancellationToken ct);
    Task AcknowledgeAsync(string batchId, CancellationToken ct);
    Task PublishAsync(Message message, CancellationToken ct);
}

public interface IDataProcessor
{
    Task<ProcessingResult> ProcessAsync(Message message, CancellationToken ct);
}
