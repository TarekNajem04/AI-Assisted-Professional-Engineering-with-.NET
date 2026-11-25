// book/samples/Chapter-03/EngineeringCommunicationToolkit/tests/Unit/PromptContractTests.cs
//
// Unit tests for S01's PromptContract<TInput, TOutput>.
// RenderSystemPrompt() and RenderUserPrompt() are pure, deterministic
// methods — no AI call needed, no stub required.

using System.Text.RegularExpressions;
using AiToolkit.Core.Prompts;
using Jeninnet.Assertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AiToolkit.Tests.Unit;

[TestClass]
public sealed class PromptContractTests
{
    private readonly CodeReviewPromptContract _contract = new();

    [TestMethod]
    public void ContractId_IsStable()
    {
        _contract.ContractId.Should().Be("code-review-comment");
    }

    [TestMethod]
    public void Version_IsSemanticVersion()
    {
        (Regex.IsMatch(_contract.Version, @"^\d+\.\d+\.\d+$")).Should().BeTrue();
    }

    [TestMethod]
    public void FullyQualifiedId_CombinesIdAndVersion()
    {
        _contract.FullyQualifiedId.Should().Be($"{_contract.ContractId}@{_contract.Version}");
    }

    [TestMethod]
    public void RenderSystemPrompt_IsNonEmpty()
    {
        var prompt = _contract.RenderSystemPrompt();
        (string.IsNullOrWhiteSpace(prompt)).Should().BeFalse();
    }

    [TestMethod]
    public void RenderSystemPrompt_IsDeterministic()
    {
        _contract.RenderSystemPrompt().Should().Be(_contract.RenderSystemPrompt());
    }

    [TestMethod]
    public void RenderUserPrompt_ContainsFileName()
    {
        var input = new CodeReviewRequest(
            FileName: "PaymentService.cs",
            Language: "csharp",
            Code: "var x = 1;",
            FocusAreas: ["correctness"]);

        var rendered = _contract.RenderUserPrompt(input);

        rendered.Should().Contain("PaymentService.cs");
    }

    [TestMethod]
    public void RenderUserPrompt_ContainsCode()
    {
        var code = "public void Foo() { }";
        var input = new CodeReviewRequest("F.cs", "csharp", code, ["style"]);

        _contract.RenderUserPrompt(input).Should().Contain(code);
    }

    [TestMethod]
    public void RenderUserPrompt_ContainsFocusAreas()
    {
        var input = new CodeReviewRequest(
            "F.cs", "csharp", "var x = 1;",
            ["performance", "security"]);

        var rendered = _contract.RenderUserPrompt(input);

        rendered.Should().Contain("performance");
        rendered.Should().Contain("security");
    }

    [TestMethod]
    public void RenderUserPrompt_IsDeterministicForSameInput()
    {
        var input = new CodeReviewRequest("F.cs", "csharp", "var x = 1;", ["style"]);
        _contract.RenderUserPrompt(input).Should().Be(_contract.RenderUserPrompt(input));
    }
}
