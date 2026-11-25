// book/samples/Chapter-03/EngineeringCommunicationToolkit/tests/Unit/WorkflowRegistryTests.cs
//
// Verifies that the DI-registered workflow resolves the STRUCTURED client
// (S05), not a free-form client. The fake factory makes this observable.

using AiToolkit.Application.Ports;
using AiToolkit.Core.Prompts;
using AiToolkit.Infrastructure.AI;
using Jeninnet.Assertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AiToolkit.Tests.Unit;

[TestClass]
public sealed class WorkflowRegistryTests
{
    [TestMethod]
    public async Task RegisteredWorkflow_UsesStructuredClient()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAiClientFactory>(
            new FakeAiClientFactory(
                freeFormOutput: "free-form output",
                structuredOutput: "{\"issues\":[]}"));
        services.AddSingleton<IAiOutputParser<string>, PassThroughStringParser>();
        services.AddSingleton<ILogger<DeterministicAiBoundary<string>>>(
            NullLogger<DeterministicAiBoundary<string>>.Instance);
        services.AddWorkflows();

        using var provider = services.BuildServiceProvider();
        var workflow = provider.GetRequiredService<IAiWorkflow<CodeReviewRequest, string>>();

        var result = await workflow.ExecuteAsync(
            new CodeReviewRequest("Demo.cs", "csharp", "var x = 1;", ["correctness"]),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("{\"issues\":[]}");
        result.WorkflowId.Should().Be("code-review@1.0");
    }
}
