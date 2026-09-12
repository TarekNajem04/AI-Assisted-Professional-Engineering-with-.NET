// book/samples/Chapter-01/DistributedPaymentService/src/PaymentService.Api/Workers/DataProcessingWorker.cs
//
// S06 concept: CLR runtime knowledge catching an AI-generated allocation issue.
//
// The AI-generated version used:
//   Task.WhenAll(batch.Messages.Select(m => _processor.ProcessAsync(m, ct)))
//
// This creates 1 Closure allocation per message per batch.
// At 50 batches/sec × 1000 messages = 50,000 Gen0 heap allocations/sec.
// GC Gen0 collections every ~100ms; ~5% throughput penalty at scale.
//
// The corrected version materialises the array once and uses a for-loop,
// eliminating all Closure allocations without changing observable behaviour.

using PaymentService.Application.Ports;

namespace PaymentService.Api.Workers;

public sealed class DataProcessingWorker(
    IMessageBus bus,
    IDataProcessor processor,
    ILogger<DataProcessingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var batch in bus.ReadBatchesAsync(stoppingToken))
        {
            // ── Architecture note (S06) ────────────────────────────────────
            // ToArray() materialises once. The for-loop avoids the LINQ
            // Select() lambda that would capture `stoppingToken` as a Closure.
            // Each Closure is a short-lived Gen0 allocation; at 50 batches/sec
            // the GC pressure becomes measurable in production profiling.
            var messages = batch.Messages.ToArray();
            var tasks    = new Task<ProcessingResult>[messages.Length];

            for (int i = 0; i < messages.Length; i++)
                tasks[i] = processor.ProcessAsync(messages[i], stoppingToken);

            var results = await Task.WhenAll(tasks);

            // Counted with a for-loop, not LINQ Count(predicate):
            // a predicate lambda would reintroduce the per-batch Closure
            // this file exists to eliminate.
            int failed = 0;
            for (int i = 0; i < results.Length; i++)
                if (!results[i].Success)
                    failed++;
            if (failed > 0)
                logger.LogWarning(
                    "Batch {BatchId}: {Failed}/{Total} messages failed",
                    batch.BatchId, failed, results.Length);

            // Acknowledge only after all processing completes.
            // Architecture note: if the process crashes after processing but
            // before acknowledgement, messages will be redelivered.
            // All downstream processing must therefore be idempotent.
            await bus.AcknowledgeAsync(batch.BatchId, stoppingToken);

            logger.LogInformation(
                "Batch {BatchId} processed: {Success}/{Total} succeeded",
                batch.BatchId,
                results.Length - failed,
                results.Length);
        }
    }
}
