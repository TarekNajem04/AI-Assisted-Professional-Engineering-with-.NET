// book/samples/Chapter-03/EngineeringCommunicationToolkit/src/AiToolkit.Infrastructure/AI/MultiStepAiSession.cs
//
// S06 concept: owns explicit, externally-managed state for a multi-step
// AI task. Composes IAiWorkflow (S03), VerificationLevel (Ch02 S06), and
// collaboration patterns (Ch01 S04) into a single auditable unit.
//
// The saga-orchestrator analogy: just as a saga orchestrator owns
// transaction state independently of any participating service's internal
// memory, MultiStepAiSession owns task state independently of the AI
// model's complete lack of persistent memory (Chapter 02, Section 01).

using Microsoft.Extensions.Logging;

namespace AiToolkit.Infrastructure.AI;

// ── Collaboration patterns (Chapter 01, Section 04) ───────────────────────

/// <summary>
/// The four collaboration models from Chapter 01 Section 04, made an
/// explicit, trackable, runtime-enforced property of each step.
/// </summary>
public enum CollaborationPattern
{
    /// <summary>Produces output for downstream steps to consume automatically.</summary>
    Generator,

    /// <summary>Produces feedback on a prior step's output; not a new artifact.</summary>
    Reviewer,

    /// <summary>Produces a recommendation requiring explicit human approval
    /// before downstream steps may depend on it.</summary>
    Advisor,

    /// <summary>Carries out a specific, bounded action.</summary>
    Executor,
}

// ── Step record ───────────────────────────────────────────────────────────

public sealed record StepRecord(
    int StepNumber,
    string StepName,
    CollaborationPattern Pattern,
    string Output,
    VerificationLevel VerificationLevel,
    bool RequiresHumanDecision,
    bool HumanApproved);

// ── VerificationLevel (Chapter 02, Section 06 — local copy) ──────────────

public enum VerificationLevel
{
    Light = 1,
    Moderate = 2,
    Deep = 3,
    Formal = 4,
}

// ── Session ───────────────────────────────────────────────────────────────

public sealed class MultiStepAiSession(
    string sessionId,
    ILogger<MultiStepAiSession> logger)
{
    private readonly List<StepRecord> _steps = [];

    public string SessionId { get; } = sessionId;
    public IReadOnlyList<StepRecord> CompletedSteps => _steps.AsReadOnly();

    public async Task<StepRecord> RunStepAsync<TInput, TOutput>(
        string stepName,
        CollaborationPattern pattern,
        IAiWorkflow<TInput, TOutput> workflow,
        TInput input,
        VerificationLevel verificationLevel,
        CancellationToken ct)
        where TOutput : notnull
    {
        var result = await workflow.ExecuteAsync(input, ct);

        if (!result.IsSuccess)
            throw new InvalidOperationException(
                $"Step '{stepName}' failed: {result.Error}");

        var requiresDecision = pattern is CollaborationPattern.Advisor;

        var record = new StepRecord(
            StepNumber: _steps.Count + 1,
            StepName: stepName,
            Pattern: pattern,
            Output: result.Value!.ToString() ?? string.Empty,
            VerificationLevel: verificationLevel,
            RequiresHumanDecision: requiresDecision,
            // Advisor steps default to NOT approved — engineer must call
            // ApproveAdvisorStep explicitly. All other patterns auto-proceed.
            HumanApproved: !requiresDecision);

        _steps.Add(record);

        logger.LogInformation(
            "Session {Id}: step {Num} '{Name}' completed. " +
            "Pattern={Pattern}, Level={Level}, Approved={Approved}",
            SessionId, record.StepNumber, stepName,
            pattern, verificationLevel, record.HumanApproved);

        return record;
    }

    /// <summary>
    /// The ONLY way an Advisor step's output is marked usable by
    /// subsequent steps — never inferred, never automatic.
    /// Enforces Chapter 01 Section 04's distinction: an advisor
    /// recommends, the engineer decides.
    /// </summary>
    public void ApproveAdvisorStep(int stepNumber, string approvedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(approvedBy);

        var index = _steps.FindIndex(s => s.StepNumber == stepNumber);

        if (index < 0)
            throw new ArgumentException(
                $"No step {stepNumber} in session '{SessionId}'.");

        if (_steps[index].Pattern is not CollaborationPattern.Advisor)
            throw new InvalidOperationException(
                $"Step {stepNumber} ('{_steps[index].StepName}') " +
                "is not an Advisor step and does not require approval.");

        _steps[index] = _steps[index] with { HumanApproved = true };

        logger.LogInformation(
            "Session {Id}: Advisor step {Num} approved by {By}",
            SessionId, stepNumber, approvedBy);
    }

    /// <summary>
    /// Guards downstream steps from proceeding on an unapproved Advisor
    /// recommendation. Makes the responsibility boundary (Chapter 01 S05)
    /// a runtime-enforced property, not a convention relying on memory.
    /// </summary>
    public void EnsureStepApproved(int stepNumber)
    {
        var step = _steps.SingleOrDefault(s => s.StepNumber == stepNumber)
            ?? throw new ArgumentException(
                $"No step {stepNumber} in session '{SessionId}'.");

        if (!step.HumanApproved)
            throw new InvalidOperationException(
                $"Step {stepNumber} ('{step.StepName}') is an unapproved " +
                "Advisor recommendation. Call ApproveAdvisorStep() before " +
                "allowing downstream steps to depend on its output.");
    }
}
