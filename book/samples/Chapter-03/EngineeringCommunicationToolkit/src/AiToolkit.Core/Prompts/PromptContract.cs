// book/samples/Chapter-03/EngineeringCommunicationToolkit/src/AiToolkit.Core/Prompts/PromptContract.cs
// S01: versioned typed prompt contract — pure rendering logic, no SDK dependency.

namespace AiToolkit.Core.Prompts;

public abstract class PromptContract<TInput, TOutput>
{
    public abstract string ContractId { get; }
    public abstract string Version { get; }
    public abstract string RenderSystemPrompt();
    public abstract string RenderUserPrompt(TInput input);
    public string FullyQualifiedId => $"{ContractId}@{Version}";
}

public sealed class CodeReviewPromptContract : PromptContract<CodeReviewRequest, string>
{
    public override string ContractId => "code-review-comment";
    public override string Version => "1.2.0";

    public override string RenderSystemPrompt() =>
        "You are a senior .NET engineer performing a code review. " +
        "Identify concrete issues only. For each issue cite the specific " +
        "line or pattern and explain the concrete risk.";

    public override string RenderUserPrompt(CodeReviewRequest input) =>
        $"""
        Review the following {input.Language} code from {input.FileName}:
        ```{input.Language}
        {input.Code}
        ```
        Focus areas: {string.Join(", ", input.FocusAreas)}
        """;
}

public sealed record CodeReviewRequest(
    string FileName, string Language, string Code, IReadOnlyList<string> FocusAreas);
