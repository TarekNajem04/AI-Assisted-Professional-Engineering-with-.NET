// book/samples/Chapter-03/EngineeringCommunicationToolkit/src/AiToolkit.Core/Tokens/TokenEstimator.cs
//
// Local copy of Chapter 02's TokenEstimator, included here so that the
// Chapter 03 sample is fully self-contained and does not require a
// project reference to the Chapter 02 sample directory.
// In a real shared repository this would be a NuGet package reference.

namespace AiToolkit.Core.Tokens;

public static class TokenEstimator
{
    private const double ProseCharsPerToken = 4.0;
    private const double CodeCharsPerToken = 3.0;

    public static int EstimateProseTokens(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (text.Length == 0) return 0;
        return (int)Math.Ceiling(text.Length / ProseCharsPerToken);
    }

    public static int EstimateCodeTokens(string code)
    {
        ArgumentNullException.ThrowIfNull(code);
        if (code.Length == 0) return 0;
        return (int)Math.Ceiling(code.Length / CodeCharsPerToken);
    }

    public static BudgetCheckResult CheckBudget(
        string systemPrompt, string contextText, string userPrompt,
        int contextWindowTokens, int reservedForResponse = 1000)
    {
        var systemTokens = EstimateProseTokens(systemPrompt);
        var contextTokens = EstimateProseTokens(contextText);
        var userTokens = EstimateProseTokens(userPrompt);
        var totalInput = systemTokens + contextTokens + userTokens;
        var available = contextWindowTokens - totalInput;

        return new BudgetCheckResult(
            Fits: available >= reservedForResponse,
            EstimatedInputTokens: totalInput,
            AvailableForResponse: Math.Max(0, available),
            SystemPromptTokens: systemTokens,
            ContextTokens: contextTokens,
            UserPromptTokens: userTokens);
    }
}

public sealed record BudgetCheckResult(
    bool Fits,
    int EstimatedInputTokens,
    int AvailableForResponse,
    int SystemPromptTokens,
    int ContextTokens,
    int UserPromptTokens);
