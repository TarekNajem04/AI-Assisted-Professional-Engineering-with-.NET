// Unit tests for S04's AiSuggestionValidator. Verifies that the
// fabrication and outdated-pattern detection categories from Section
// 04's taxonomy are correctly caught.

using AiToolkit.Infrastructure.AI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jeninnet.Assertions;

namespace AiToolkit.Tests.Unit.AI;

[TestClass]
public sealed class AiSuggestionValidatorTests {
    private readonly AiSuggestionValidator _validator = new();

    [TestMethod]
    public void Validate_CleanModernCode_ReturnsValid() {
        const string code = """
            var client = new HttpClient();
            var response = await client.GetAsync(url, cancellationToken);
            """;

        var result = _validator.Validate(code);

        (result.IsValid).Should().BeTrue();
    }

    [TestMethod]
    public void Validate_FabricatedNamespace_ReturnsError() {
        const string code = "using System.Web.Http;\npublic class Foo { }";

        var result = _validator.Validate(code);

        (result.IsValid).Should().BeFalse();
        (result.Issues!.Any(i => i.Code == "AI001" && i.Severity == Severity.Error)).Should().BeTrue();
    }

    [TestMethod]
    public void Validate_OutdatedThreadSleep_ReturnsWarning() {
        const string code = "Thread.Sleep(1000);";

        var result = _validator.Validate(code);

        (result.IsValid).Should().BeFalse();
        (result.Issues!.Any(i => i.Code == "AI002" && i.Severity == Severity.Warning)).Should().BeTrue();
    }

    [TestMethod]
    public void Validate_SyncOverAsyncResult_ReturnsWarning() {
        const string code = "var data = SomeAsyncCall().Result;";

        var result = _validator.Validate(code);

        (result.IsValid).Should().BeFalse();
        (result.Issues!.Any(i => i.Code == "AI002")).Should().BeTrue();
    }

    [TestMethod]
    public void Validate_MultipleIssues_ReturnsAllOfThem() {
        const string code = """
            using System.Web.Http;
            Thread.Sleep(500);
            var x = GetAsync().Result;
            """;

        var result = _validator.Validate(code);

        (result.IsValid).Should().BeFalse();
        (result.Issues!.Count >= 3).Should().BeTrue();
    }

    [TestMethod]
    public void Validate_EmptyCode_ReturnsValid() {
        var result = _validator.Validate(string.Empty);

        (result.IsValid).Should().BeTrue();
    }

    [TestMethod]
    public void Validate_NullCode_ThrowsArgumentNullException() {
        FluentActions.Invoking(() => { _validator.Validate(null!); }).Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void Validate_HttpWebRequestFabrication_ReturnsWarning() {
        const string code = "var req = (HttpWebRequest)WebRequest.Create(url);";

        var result = _validator.Validate(code);

        (result.IsValid).Should().BeFalse();
        (result.Issues!.Any(i => i.Message.Contains("HttpWebRequest"))).Should().BeTrue();
    }
}
