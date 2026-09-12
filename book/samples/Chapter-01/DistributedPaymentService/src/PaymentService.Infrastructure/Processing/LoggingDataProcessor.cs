// book/samples/Chapter-01/DistributedPaymentService/src/PaymentService.Infrastructure/Processing/LoggingDataProcessor.cs
using Microsoft.Extensions.Logging;
using PaymentService.Application.Ports;
using System.Text.Json;

namespace PaymentService.Infrastructure.Processing;

public sealed class LoggingDataProcessor(ILogger<LoggingDataProcessor> logger) : IDataProcessor
{
    public async Task<ProcessingResult> ProcessAsync(Message message, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(message);

        await Task.Yield();

        try
        {
            if (string.IsNullOrWhiteSpace(message.Payload))
                return new ProcessingResult(false, message.Id, "EMPTY_PAYLOAD");

            using var doc = JsonDocument.Parse(message.Payload);
            logger.LogInformation("Processed message {MessageId} on topic {Topic}", message.Id, message.Topic);
            return new ProcessingResult(true, message.Id);
        }
        catch (JsonException ex)
        {
            logger.LogDebug("Message {MessageId} is plain-text payload: {Hint}", message.Id, ex.Message);
            return new ProcessingResult(true, message.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process message {MessageId}", message.Id);
            return new ProcessingResult(false, message.Id, ex.Message);
        }
    }
}
