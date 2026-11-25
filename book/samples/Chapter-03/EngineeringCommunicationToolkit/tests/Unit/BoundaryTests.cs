// book/samples/Chapter-03/EngineeringCommunicationToolkit/tests/Unit/BoundaryTests.cs
//
// S03 boundary tests: provider failures and parse failures must become
// deterministic results, not unhandled exceptions.

using AiToolkit.Infrastructure.AI;
using Jeninnet.Assertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AiToolkit.Tests.Unit;

[TestClass]
public sealed class BoundaryTests
{
    [TestMethod]
    public async Task ExecuteAsync_ProviderFailure_ReturnsFailureResult()
    {
        var client = new StubChatClient(
            (_, _, _) => throw new InvalidOperationException("provider down"));
        var boundary = new DeterministicAiBoundary<string>(
            client,
            new PassThroughStringParser(),
            NullLogger<DeterministicAiBoundary<string>>.Instance);

        var result = await boundary.ExecuteAsync("system", "user", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Should().Contain("AI provider failure");
    }

    [TestMethod]
    public async Task ExecuteAsync_ParseFailure_RetriesThenSucceeds()
    {
        var client = new StubChatClient(
            (_, _, _) => Task.FromResult("valid output"));
        var parser = new FailOnceParser();
        var boundary = new DeterministicAiBoundary<string>(
            client,
            parser,
            NullLogger<DeterministicAiBoundary<string>>.Instance);

        var result = await boundary.ExecuteAsync("system", "user", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("valid output");
        parser.Calls.Should().Be(2);
    }

    [TestMethod]
    public async Task ExecuteAsync_CancelledRequest_ThrowsOperationCanceled()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var client = new StubChatClient(
            (_, _, ct) =>
            {
                ct.ThrowIfCancellationRequested();
                return Task.FromResult("never used");
            });
        var boundary = new DeterministicAiBoundary<string>(
            client,
            new PassThroughStringParser(),
            NullLogger<DeterministicAiBoundary<string>>.Instance);

        await FluentActions.Awaiting(
                () => boundary.ExecuteAsync("system", "user", cts.Token))
            .Should().ThrowAsync<OperationCanceledException>();
    }
}
