// book/samples/Chapter-03/EngineeringCommunicationToolkit/tests/Unit/MultiStepAiSessionTests.cs
//
// Unit tests for S06's MultiStepAiSession — state management and the
// approval-gate logic. All tests use stubs; no live AI call is needed,
// demonstrating that the orchestration logic is fully deterministic
// (Section 06's architectural point: the probabilistic component is
// isolated inside each IAiWorkflow, not the session itself).

using AiToolkit.Infrastructure.AI;
using Jeninnet.Assertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AiToolkit.Tests.Unit;

[TestClass]
public sealed class MultiStepAiSessionTests
{
    // ── Helpers ──────────────────────────────────────────────────────────

    private static MultiStepAiSession CreateSession(string id = "test-session")
        => new(id, NullLogger<MultiStepAiSession>.Instance);

    private static IAiWorkflow<string, string> StubWorkflow(string output = "ok") =>
        new StubStringWorkflow(output);

    // ── State management ─────────────────────────────────────────────────

    [TestMethod]
    public async Task RunStepAsync_CompletedSteps_IsRecorded()
    {
        var session = CreateSession();
        await session.RunStepAsync(
            "step-1", CollaborationPattern.Generator,
            StubWorkflow("result"), "input",
            VerificationLevel.Light, CancellationToken.None);

        session.CompletedSteps.Count.Should().Be(1);
        session.CompletedSteps[0].StepName.Should().Be("step-1");
    }

    [TestMethod]
    public async Task RunStepAsync_StepNumbers_AreSequential()
    {
        var session = CreateSession();
        await session.RunStepAsync("s1", CollaborationPattern.Generator,
            StubWorkflow(), "in", VerificationLevel.Light, default);
        await session.RunStepAsync("s2", CollaborationPattern.Reviewer,
            StubWorkflow(), "in", VerificationLevel.Light, default);

        session.CompletedSteps[0].StepNumber.Should().Be(1);
        session.CompletedSteps[1].StepNumber.Should().Be(2);
    }

    [TestMethod]
    public async Task RunStepAsync_GeneratorStep_IsAutoApproved()
    {
        var session = CreateSession();
        var step = await session.RunStepAsync(
            "gen", CollaborationPattern.Generator,
            StubWorkflow(), "in", VerificationLevel.Light, default);

        (step.HumanApproved).Should().BeTrue();
        (step.RequiresHumanDecision).Should().BeFalse();
    }

    [TestMethod]
    public async Task RunStepAsync_AdvisorStep_RequiresApproval()
    {
        var session = CreateSession();
        var step = await session.RunStepAsync(
            "advise", CollaborationPattern.Advisor,
            StubWorkflow(), "in", VerificationLevel.Moderate, default);

        (step.HumanApproved).Should().BeFalse();
        (step.RequiresHumanDecision).Should().BeTrue();
    }

    [TestMethod]
    public async Task RunStepAsync_VerificationLevel_IsRecorded()
    {
        var session = CreateSession();
        var step = await session.RunStepAsync(
            "deep-step", CollaborationPattern.Reviewer,
            StubWorkflow(), "in", VerificationLevel.Deep, default);

        step.VerificationLevel.Should().Be(VerificationLevel.Deep);
    }

    // ── Approval gate ─────────────────────────────────────────────────────

    [TestMethod]
    public async Task ApproveAdvisorStep_SetsHumanApprovedTrue()
    {
        var session = CreateSession();
        var step = await session.RunStepAsync(
            "advise", CollaborationPattern.Advisor,
            StubWorkflow(), "in", VerificationLevel.Moderate, default);

        session.ApproveAdvisorStep(step.StepNumber, "engineer@example.com");

        (session.CompletedSteps[0].HumanApproved).Should().BeTrue();
    }

    [TestMethod]
    public async Task EnsureStepApproved_AfterApproval_DoesNotThrow()
    {
        var session = CreateSession();
        var step = await session.RunStepAsync(
            "advise", CollaborationPattern.Advisor,
            StubWorkflow(), "in", VerificationLevel.Moderate, default);

        session.ApproveAdvisorStep(step.StepNumber, "engineer");

        session.EnsureStepApproved(step.StepNumber);
    }

    [TestMethod]
    public async Task EnsureStepApproved_BeforeApproval_Throws()
    {
        var session = CreateSession();
        var step = await session.RunStepAsync(
            "advise", CollaborationPattern.Advisor,
            StubWorkflow(), "in", VerificationLevel.Moderate, default);

        FluentActions.Invoking(() => { session.EnsureStepApproved(step.StepNumber); }).Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    public async Task ApproveAdvisorStep_OnNonAdvisorStep_Throws()
    {
        var session = CreateSession();
        var step = await session.RunStepAsync(
            "gen", CollaborationPattern.Generator,
            StubWorkflow(), "in", VerificationLevel.Light, default);

        FluentActions.Invoking(() => { session.ApproveAdvisorStep(step.StepNumber, "engineer"); }).Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    public void ApproveAdvisorStep_NonExistentStep_Throws()
    {
        var session = CreateSession();
        FluentActions.Invoking(() => { session.ApproveAdvisorStep(99, "engineer"); }).Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public async Task RunStepAsync_FailedWorkflow_Throws()
    {
        var session = CreateSession();
        var failing = new FailingWorkflow();

        await FluentActions.Awaiting(() =>
            session.RunStepAsync(
                "fail", CollaborationPattern.Generator,
                failing, "in", VerificationLevel.Light, default)).Should().ThrowAsync<InvalidOperationException>();
    }

    // ── Collaboration pattern tracking ────────────────────────────────────

    [TestMethod]
    public void RunStepAsync_GeneratorPattern_DoesNotRequireDecision()
    {
        RunStepAsync_NonAdvisorPattern_DoesNotRequireDecision(CollaborationPattern.Generator).GetAwaiter().GetResult();
    }

    [TestMethod]
    public void RunStepAsync_ReviewerPattern_DoesNotRequireDecision()
    {
        RunStepAsync_NonAdvisorPattern_DoesNotRequireDecision(CollaborationPattern.Reviewer).GetAwaiter().GetResult();
    }

    [TestMethod]
    public void RunStepAsync_ExecutorPattern_DoesNotRequireDecision()
    {
        RunStepAsync_NonAdvisorPattern_DoesNotRequireDecision(CollaborationPattern.Executor).GetAwaiter().GetResult();
    }

    private async Task RunStepAsync_NonAdvisorPattern_DoesNotRequireDecision(
        CollaborationPattern pattern)
    {
        var session = CreateSession();
        var step = await session.RunStepAsync(
            "s", pattern, StubWorkflow(), "in",
            VerificationLevel.Light, default);

        (step.RequiresHumanDecision).Should().BeFalse();
        (step.HumanApproved).Should().BeTrue();
    }

    // ── Test doubles ──────────────────────────────────────────────────────

    private sealed class StubStringWorkflow(string output) : IAiWorkflow<string, string>
    {
        public string WorkflowId => "stub-workflow";

        public Task<WorkflowResult<string>> ExecuteAsync(
            string input, CancellationToken ct) =>
            Task.FromResult(new WorkflowResult<string>(
                IsSuccess: true,
                Value: output,
                Error: null,
                WorkflowId: WorkflowId,
                ContractVersion: "stub@1.0"));
    }

    private sealed class FailingWorkflow : IAiWorkflow<string, string>
    {
        public string WorkflowId => "failing-workflow";

        public Task<WorkflowResult<string>> ExecuteAsync(
            string input, CancellationToken ct) =>
            Task.FromResult(new WorkflowResult<string>(
                IsSuccess: false,
                Value: null,
                Error: "Simulated validation failure",
                WorkflowId: WorkflowId,
                ContractVersion: "failing@1.0"));
    }
}
