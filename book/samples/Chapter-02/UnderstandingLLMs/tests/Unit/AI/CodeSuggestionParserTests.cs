// Unit tests for CodeSuggestionParser — the glue between S04's
// validator and S05's boundary. Covers fence extraction edge cases:
// missing fences, unterminated fences, and validation-failure
// propagation as typed results (never exceptions).

using AiToolkit.Application.Ports;
using AiToolkit.Infrastructure.AI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jeninnet.Assertions;

namespace AiToolkit.Tests.Unit.AI;

[TestClass]
public sealed class CodeSuggestionParserTests {
    private static CodeSuggestionParser CreateParser() =>
        new(new AiSuggestionValidator());

    [TestMethod]
    public void Parse_FencedCleanCode_ReturnsCodeAndExplanation() {
        var parser = CreateParser();
        const string raw = """
            Here is the implementation:
            ```csharp
            var client = new HttpClient();
            ```
            Uses a single shared client instance.
            """;

        var result = parser.Parse(raw);

        (result.IsSuccess).Should().BeTrue();
        (result.Value!.Code).Should().Contain("HttpClient");
        (result.Value.Explanation).Should().Contain("shared client");
    }

    [TestMethod]
    public void Parse_NoFence_ValidatesWholeText() {
        var parser = CreateParser();

        var result = parser.Parse("var client = new HttpClient();");

        (result.IsSuccess).Should().BeTrue();
    }

    [TestMethod]
    public void Parse_UnterminatedFence_FallsBackToWholeText() {
        var parser = CreateParser();

        // Unterminated fence: no code block extracted, so the whole text
        // is validated as-is. Nothing fabricated here, so it succeeds —
        // the point is a typed result either way, never an unhandled throw.
        var result = parser.Parse("```csharp\nvar x = 1;");

        (result.IsSuccess).Should().BeTrue();
    }

    [TestMethod]
    public void Parse_EmptyResponse_ReturnsTypedFailure() {
        var parser = CreateParser();

        var result = parser.Parse("   ");

        (result.IsSuccess).Should().BeFalse();
        (result.Error).Should().NotBeNull();
    }

    [TestMethod]
    public void Parse_FabricatedNamespaceInsideFence_FailsWithAi001() {
        var parser = CreateParser();
        const string raw = """
            ```csharp
            using System.Web.Http;
            public class Foo { }
            ```
            """;

        var result = parser.Parse(raw);

        (result.IsSuccess).Should().BeFalse();
        (result.Error!).Should().Contain("AI001");
    }
}
