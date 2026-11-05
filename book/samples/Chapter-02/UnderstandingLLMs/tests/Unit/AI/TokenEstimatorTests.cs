// Unit tests for S01's TokenEstimator. Pure calculation, fully
// deterministic — verifies the budget-allocation primitive behaves
// sanely at the boundaries that matter (empty input, prose vs code
// rates, headroom accounting).

using AiToolkit.Core.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jeninnet.Assertions;

namespace AiToolkit.Tests.Unit.AI;

[TestClass]
public sealed class TokenEstimatorTests {
    [TestMethod]
    public void EstimateProseTokens_EmptyString_ReturnsZero() {
        TokenEstimator.EstimateProseTokens(string.Empty).Should().Be(0);
    }

    [TestMethod]
    public void EstimateCodeTokens_EmptyString_ReturnsZero() {
        TokenEstimator.EstimateCodeTokens(string.Empty).Should().Be(0);
    }

    [TestMethod]
    public void EstimateCodeTokens_DenseCode_EstimatesMoreThanProse() {
        const string text = "var x = new Dictionary<string, List<int>>();";
        TokenEstimator.EstimateCodeTokens(text)
            .Should().BeGreaterThanOrEqualTo(TokenEstimator.EstimateProseTokens(text));
    }

    [TestMethod]
    public void CheckBudget_RoomyWindow_FitsWithHeadroom() {
        var result = TokenEstimator.CheckBudget(
            systemPrompt: "sys",
            contextText: "ctx",
            userPrompt: "user?",
            contextWindowTokens: 128_000,
            reservedForResponse: 1000);

        (result.Fits).Should().BeTrue();
        (result.AvailableForResponse >= 1000).Should().BeTrue();
        (result.EstimatedInputTokens).Should().Be(
            result.SystemPromptTokens + result.ContextTokens + result.UserPromptTokens);
    }

    [TestMethod]
    public void CheckBudget_TinyWindow_DoesNotFitAndClampsAtZero() {
        var result = TokenEstimator.CheckBudget(
            systemPrompt: new string('s', 4000),
            contextText: new string('c', 4000),
            userPrompt: new string('u', 4000),
            contextWindowTokens: 100,
            reservedForResponse: 1000);

        (result.Fits).Should().BeFalse();
        (result.AvailableForResponse).Should().Be(0);
    }

    [TestMethod]
    public void EstimateProseTokens_NullInput_ThrowsArgumentNullException() {
        FluentActions.Invoking(() => { TokenEstimator.EstimateProseTokens(null!); })
            .Should().Throw<ArgumentNullException>();
    }
}
