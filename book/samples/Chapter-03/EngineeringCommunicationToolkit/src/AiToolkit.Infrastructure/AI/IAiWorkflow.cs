// book/samples/Chapter-03/EngineeringCommunicationToolkit/src/AiToolkit.Infrastructure/AI/IAiWorkflow.cs
// S03: reusable workflow packaging via DI.
using AiToolkit.Application.Ports;
using AiToolkit.Core.Prompts;
using Microsoft.Extensions.Logging;

namespace AiToolkit.Infrastructure.AI;

public interface IAiWorkflow<TInput, TOutput>
{
    string WorkflowId { get; }
    Task<WorkflowResult<TOutput>> ExecuteAsync(TInput input, CancellationToken ct);
}

public sealed record WorkflowResult<T>(
    bool IsSuccess, T? Value, string? Error, string WorkflowId, string ContractVersion);

public sealed class AiWorkflow<TInput, TOutput>(
    string workflowId,
    PromptContract<TInput, TOutput> contract,
    Func<TInput, ContextBuilder> contextStrategy,
    DeterministicAiBoundary<TOutput> boundary)
    : IAiWorkflow<TInput, TOutput>
{
    public string WorkflowId { get; } = workflowId;

    public async Task<WorkflowResult<TOutput>> ExecuteAsync(TInput input, CancellationToken ct)
    {
        var ctx = contextStrategy(input).Build();
        var sys = contract.RenderSystemPrompt();
        var usr = $"{contract.RenderUserPrompt(input)}\n\nRelevant context:\n{ctx.AssembledContext}";
        var r = await boundary.ExecuteAsync(sys, usr, ct);

        return new WorkflowResult<TOutput>(
            r.IsSuccess, r.Value, r.Error, WorkflowId, contract.FullyQualifiedId);
    }
}

public sealed class DeterministicAiBoundary<TResult>(
    IChatCompletionClient chatClient,
    IAiOutputParser<TResult> parser,
    ILogger<DeterministicAiBoundary<TResult>> logger)
{
    private const int MaxAttempts = 3;

    public async Task<BoundaryResult<TResult>> ExecuteAsync(
        string sys, string usr, CancellationToken ct)
    {
        string? lastErr = null;

        for (var i = 1; i <= MaxAttempts; i++)
        {
            var effective = lastErr is null
                ? usr
                : $"{usr}\n\nIMPORTANT: Previous issue: {lastErr}. Please correct.";

            string raw;
            try
            {
                raw = await chatClient.CompleteAsync(sys, effective, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Provider/network failures are expected boundary inputs,
                // not unhandled application exceptions.
                lastErr = $"AI provider failure: {ex.Message}";
                logger.LogWarning(
                    "Boundary attempt {A} failed with provider exception: {E}", i, lastErr);
                continue;
            }

            var parsed = parser.Parse(raw);
            if (parsed.IsSuccess)
            {
                logger.LogInformation("Boundary success on attempt {A}", i);
                return BoundaryResult<TResult>.Success(parsed.Value!);
            }

            lastErr = parsed.Error;
            logger.LogWarning("Boundary attempt {A} failed: {E}", i, lastErr);
        }

        return BoundaryResult<TResult>.Failure(lastErr!);
    }
}

public sealed record BoundaryResult<T>
{
    public bool IsSuccess { get; private init; }
    public T? Value { get; private init; }
    public string? Error { get; private init; }

    public static BoundaryResult<T> Success(T v) => new() { IsSuccess = true, Value = v };
    public static BoundaryResult<T> Failure(string e) => new() { IsSuccess = false, Error = e };
}
