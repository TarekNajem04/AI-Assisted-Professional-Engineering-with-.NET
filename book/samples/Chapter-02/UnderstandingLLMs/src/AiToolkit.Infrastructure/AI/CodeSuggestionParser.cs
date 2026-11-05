// Concrete IAiOutputParser<CodeSuggestion> that wires AiSuggestionValidator
// (S04) into the DeterministicAiBoundary<T> (S05) pipeline.
// This is the glue that makes Section 04's validation and Section 05's deterministic
// seam work together as a single coherent pipeline, matching the
// AiSuggestionEndpoint's S04-S06 scope in the blueprint.

using AiToolkit.Application.Ports;

namespace AiToolkit.Infrastructure.AI;

public sealed record CodeSuggestion(string Code, string Explanation);

public sealed class CodeSuggestionParser(IAiSuggestionValidator validator)
    : IAiOutputParser<CodeSuggestion> {
    public ParseResult<CodeSuggestion> Parse(string raw) {
        if(string.IsNullOrWhiteSpace(raw)) {
            return ParseResult<CodeSuggestion>.Failure("Empty response from model.");
        }

        // Structural validation: extract a fenced code block if present.
        var code = ExtractCodeBlock(raw) ?? raw;

        // Semantic validation: run S04's fabrication/outdated-pattern checks.
        var validation = validator.Validate(code);
        if(!validation.IsValid) {
            var errorMessages = string.Join(
                "; ", validation.Issues!.Select(i => $"{i.Code}: {i.Message}"));
            return ParseResult<CodeSuggestion>.Failure($"Validation failed: {errorMessages}");
        }

        var explanation = StripCodeBlock(raw);
        return ParseResult<CodeSuggestion>.Success(new CodeSuggestion(code, explanation));
    }

    private static string? ExtractCodeBlock(string text) {
        const string fence = "```";
        var start = text.IndexOf(fence, StringComparison.Ordinal);
        if(start < 0) {
            return null;
        }

        var afterFence = text.IndexOf('\n', start);
        if(afterFence < 0) {
            return null;
        }

        var end = text.IndexOf(fence, afterFence, StringComparison.Ordinal);
        if(end < 0) {
            return null;
        }

        return text[(afterFence + 1)..end].Trim();
    }

    private static string StripCodeBlock(string text) {
        const string fence = "```";
        var start = text.IndexOf(fence, StringComparison.Ordinal);
        if(start < 0) {
            return text.Trim();
        }

        var end = text.IndexOf(fence, start + fence.Length, StringComparison.Ordinal);
        if(end < 0) {
            return text.Trim();
        }

        return (text[..start] + text[(end + fence.Length)..]).Trim();
    }
}
