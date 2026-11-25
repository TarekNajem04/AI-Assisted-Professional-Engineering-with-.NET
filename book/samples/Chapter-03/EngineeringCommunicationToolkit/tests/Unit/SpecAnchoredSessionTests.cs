// book/samples/Chapter-03/EngineeringCommunicationToolkit/tests/Unit/SpecAnchoredSessionTests.cs
//
// S04 tests: deterministic drift detection, explicit reconciliation and
// structured provider-failure handling.

using AiToolkit.Infrastructure.AI;
using Jeninnet.Assertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AiToolkit.Tests.Unit;

[TestClass]
public sealed class SpecAnchoredSessionTests
{
    [TestMethod]
    public async Task RunTurnAsync_ConsistentOutput_NoDrift()
    {
        var session = CreateSession("C01: the output stays consistent.");

        var turn = await session.RunTurnAsync("review the change", CancellationToken.None);

        turn.IsSuccess.Should().BeTrue();
        (turn.DriftDetected).Should().BeFalse();
        (turn.ClausesCited.Contains("C01")).Should().BeTrue();
    }

    [TestMethod]
    public async Task RunTurnAsync_ContradictionSignal_DriftDetected()
    {
        var session = CreateSession("C01 violates C02 by ignoring clause C02.");

        var turn = await session.RunTurnAsync("review the change", CancellationToken.None);

        (turn.DriftDetected).Should().BeTrue();
        turn.DriftDescription!.Should().Contain("violates");
    }

    [TestMethod]
    public async Task ReconcileDrift_ReviseSpecification_UpdatesActiveSpec()
    {
        var session = CreateSession("C01 contradicts C02.");
        var turn = await session.RunTurnAsync("review the change", CancellationToken.None);

        session.ReconcileDrift(
            turn.TurnNumber,
            DriftResolution.ReviseSpecification,
            "Clause C02 was ambiguous.",
            new SpecClause("C02", "Output must state one concrete risk."));

        var reconciled = session.Turns.Single(t => t.TurnNumber == turn.TurnNumber);
        (reconciled.Resolution == DriftResolution.ReviseSpecification).Should().BeTrue();
        reconciled.ReconciliationReason!.Should().Contain("ambiguous");
        session.ActiveSpecification
            .Single(c => c.Id == "C02")
            .Statement.Should().Be("Output must state one concrete risk.");
    }

    [TestMethod]
    public async Task ReconcileDrift_NoDrift_Throws()
    {
        var session = CreateSession("C01: consistent.");
        var turn = await session.RunTurnAsync("review", CancellationToken.None);

        FluentActions.Invoking(() => session.ReconcileDrift(
                turn.TurnNumber,
                DriftResolution.CorrectOutput,
                "no reason needed"))
            .Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    public async Task RunTurnAsync_ProviderFailure_ReturnsStructuredFailure()
    {
        var session = CreateSession(
            (_, _, _) => throw new InvalidOperationException("provider down"));

        var turn = await session.RunTurnAsync("review", CancellationToken.None);

        (turn.IsSuccess).Should().BeFalse();
        turn.Error!.Should().Contain("AI provider failure");
    }

    [TestMethod]
    public async Task RunTurnAsync_CustomDetector_IsUsed()
    {
        var session = CreateSession("C01: consistent.", new AlwaysDriftDetector());

        var turn = await session.RunTurnAsync("review", CancellationToken.None);

        (turn.DriftDetected).Should().BeTrue();
        turn.DriftDescription.Should().Be("Injected detector result.");
    }

    private static SpecAnchoredSession CreateSession(
        string output,
        ISpecDriftDetector? detector = null) =>
        CreateSession((_, _, _) => Task.FromResult(output), detector);

    private static SpecAnchoredSession CreateSession(
        Func<string, string, CancellationToken, Task<string>> handler,
        ISpecDriftDetector? detector = null)
    {
        var boundary = new DeterministicAiBoundary<string>(
            new StubChatClient(handler),
            new PassThroughStringParser(),
            NullLogger<DeterministicAiBoundary<string>>.Instance);

        IReadOnlyList<SpecClause> clauses =
        [
            new SpecClause("C01", "Output must cite a file."),
            new SpecClause("C02", "Output must state a risk."),
        ];

        return new SpecAnchoredSession(
            clauses,
            boundary,
            NullLogger<SpecAnchoredSession>.Instance,
            detector);
    }

    private sealed class AlwaysDriftDetector : ISpecDriftDetector
    {
        public DriftCheck Check(string output, IReadOnlyList<SpecClause> clauses) =>
            new(true, "Injected detector result.");
    }
}
