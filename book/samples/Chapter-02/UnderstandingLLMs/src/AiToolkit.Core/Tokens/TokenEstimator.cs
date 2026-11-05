// book/samples/Chapter-02/UnderstandingLLMs/src/AiToolkit.Core/Tokens/TokenEstimator.cs
//
// S01 concept: token cost estimation. As Section 01 establishes, cost and
// context consumption are measured in TOKENS, not characters or words, and
// the relationship between source text and token count is uneven —
// dependent on how well the tokenizer's vocabulary matches the text being
// encoded. This class provides a fast, dependency-free ESTIMATE (not an
// exact count — exact counts require the model-specific tokenizer) that is
// good enough for budget-allocation decisions before a request is sent.

namespace AiToolkit.Core.Tokens;

/// <summary>
/// Estimates token counts for budget-allocation purposes.
/// This is NOT an exact tokenizer — exact token counts require the
/// model-specific vocabulary (e.g. tiktoken for OpenAI models). This
/// estimator exists for fast, dependency-free pre-flight budget checks,
/// per Section 01's point that token cost must be estimated BEFORE a
/// request is constructed, not discovered after the call fails.
/// </summary>
public static class TokenEstimator {
    // Empirically, English prose averages ~4 characters per token.
    // Code with dense identifiers, GUIDs, or unusual naming consumes
    // more tokens per character because the tokenizer falls back to
    // shorter fragments more often (Section 01's BPE explanation).
    private const double ProseCharsPerToken = 4.0;
    private const double CodeCharsPerToken = 3.0;

    /// <summary>
    /// Estimates token count for plain prose text.
    /// </summary>
    public static int EstimateProseTokens(string text) {
        ArgumentNullException.ThrowIfNull(text);
        if(text.Length == 0) {
            return 0;
        }

        return (int)Math.Ceiling(text.Length / ProseCharsPerToken);
    }

    /// <summary>
    /// Estimates token count for source code, which the tokenizer
    /// typically encodes less efficiently than prose due to
    /// project-specific identifiers, punctuation density, and
    /// less-common character sequences (Section 01's
    /// "IIdempotencyKeyGenerator" example).
    /// </summary>
    public static int EstimateCodeTokens(string code) {
        ArgumentNullException.ThrowIfNull(code);
        if(code.Length == 0) {
            return 0;
        }

        return (int)Math.Ceiling(code.Length / CodeCharsPerToken);
    }

    /// <summary>
    /// Estimates whether a combined prompt (system + context + user)
    /// fits within a given context window budget, leaving headroom
    /// for the response. Returns false if the estimated input alone
    /// would leave less than <paramref name="reservedForResponse"/>
    /// tokens for generation — the "fixed-size buffer" constraint
    /// Section 01 describes.
    /// </summary>
    public static BudgetCheckResult CheckBudget(
        string systemPrompt,
        string contextText,
        string userPrompt,
        int contextWindowTokens,
        int reservedForResponse = 1000) {
        var systemTokens = EstimateProseTokens(systemPrompt);
        var contextTokens = EstimateProseTokens(contextText);
        var userTokens = EstimateProseTokens(userPrompt);
        var totalInput = systemTokens + contextTokens + userTokens;

        var availableForResponse = contextWindowTokens - totalInput;
        var fits = availableForResponse >= reservedForResponse;

        return new BudgetCheckResult(
            Fits: fits,
            EstimatedInputTokens: totalInput,
            AvailableForResponse: Math.Max(0, availableForResponse),
            SystemPromptTokens: systemTokens,
            ContextTokens: contextTokens,
            UserPromptTokens: userTokens);
    }
}

/// <summary>
/// Result of a pre-flight token budget check, per Section 01's point
/// that the context window is a hard resource constraint, not a soft
/// guideline — this type makes that constraint explicit and checkable
/// before a request is sent rather than discovered as a runtime error.
/// </summary>
public sealed record BudgetCheckResult(
    bool Fits,
    int EstimatedInputTokens,
    int AvailableForResponse,
    int SystemPromptTokens,
    int ContextTokens,
    int UserPromptTokens);
