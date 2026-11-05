// book/samples/Chapter-02/UnderstandingLLMs/src/AiToolkit.Infrastructure/AI/DeterministicAiBoundary.cs
//
// S05 concept: wraps a probabilistic AI call in a deterministic .NET
// contract. Implements validate → retry-with-feedback → typed result.
// The AI call inside is probabilistic; everything outside this class is
// deterministic and unit-testable by injecting a stub IChatCompletionClient.

using AiToolkit.Application.Ports;
using Microsoft.Extensions.Logging;

namespace AiToolkit.Infrastructure.AI;

/// <summary>
/// Enforces a deterministic typed boundary around a probabilistic AI call.
/// Callers receive either a validated typed result or a typed failure —
/// never raw AI text. Internal retry logic is invisible to callers.
/// </summary>
/// <param name="chatClient">The chat completion client.</param>
/// <param name="parser">The AI output parser.</param>
/// <param name="logger">The logger.</param>
public sealed class DeterministicAiBoundary<TResult>(
    IChatCompletionClient chatClient,
    IAiOutputParser<TResult> parser,
    ILogger<DeterministicAiBoundary<TResult>> logger) {
    private const int MAX_ATTEMPTS = 3;

    public async Task<BoundaryResult<TResult>> ExecuteAsync(
        string systemPrompt, string userPrompt, CancellationToken ct) {
        string? lastValidationError = null;

        for(var attempt = 1; attempt <= MAX_ATTEMPTS; attempt++) {
            // On retry, feed the validation failure back into the prompt.
            // The prompt construction itself is deterministic (same inputs
            // = same prompt); only the AI response is probabilistic.
            var effectiveUser = lastValidationError is null
                ? userPrompt
                : $"{userPrompt}\n\nIMPORTANT: Your previous response had " +
                  $"this issue: {lastValidationError}. Please correct it.";

            var raw = await chatClient.CompleteAsync(systemPrompt, effectiveUser, ct);

            var parseResult = parser.Parse(raw);
            if(parseResult.IsSuccess) {
                logger.LogInformation(
                    "AI boundary: success on attempt {Attempt}", attempt);
                return BoundaryResult<TResult>.Success(parseResult.Value!);
            }

            lastValidationError = parseResult.Error;
            logger.LogWarning(
                "AI boundary: attempt {Attempt} failed validation — {Error}",
                attempt, lastValidationError);
        }

        logger.LogError(
            "AI boundary: all {Max} attempts failed. Last error: {Error}",
            MAX_ATTEMPTS, lastValidationError);

        return BoundaryResult<TResult>.Failure(lastValidationError!);
    }
}

public sealed record BoundaryResult<T> {
    public bool IsSuccess { get; private init; }
    public T? Value { get; private init; }
    public string? Error { get; private init; }

    public static BoundaryResult<T> Success(T value) =>
        new() { IsSuccess = true, Value = value };

    public static BoundaryResult<T> Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
