// book/samples/Chapter-03/EngineeringCommunicationToolkit/src/AiToolkit.Infrastructure/AI/SpecAnchoredSession.cs
//
// S04 concept: anchors a multi-turn AI interaction to a written,
// decomposed specification. Drift is detected deterministically and must be
// reconciled through one of the chapter's three explicit paths before the
// turn is treated as settled.

using AiToolkit.Application.Ports;
using Microsoft.Extensions.Logging;

namespace AiToolkit.Infrastructure.AI;

public sealed record SpecClause(string Id, string Statement);

public enum DriftResolution
{
    CorrectOutput,
    ReviseSpecification,
    DecomposeClause,
}

public sealed record DriftCheck(bool Detected, string? Description);

/// <summary>
/// Deterministic, injectable drift check. A production implementation can
/// replace this with the chapter's dedicated consistency-check workflow.
/// </summary>
public interface ISpecDriftDetector
{
    DriftCheck Check(string output, IReadOnlyList<SpecClause> clauses);
}

/// <summary>
/// Sample detector: flags explicit contradiction language. It is intentionally
/// simple and deterministic; the chapter's point is that detection is an
/// owned, testable component, not that the heuristic is complete.
/// </summary>
public sealed class KeywordSpecDriftDetector : ISpecDriftDetector
{
    private static readonly string[] Signals =
    [
        "contradicts", "violates", "will not", "cannot satisfy",
        "ignoring clause", "disregard",
    ];

    public DriftCheck Check(string output, IReadOnlyList<SpecClause> clauses)
    {
        foreach (var signal in Signals)
        {
            if (output.Contains(signal, StringComparison.OrdinalIgnoreCase))
                return new DriftCheck(
                    true,
                    $"Output contains explicit contradiction signal '{signal}'.");
        }

        return new DriftCheck(false, null);
    }
}

public sealed record TurnRecord(
    int TurnNumber,
    string UserInput,
    string AiOutput,
    IReadOnlyList<string> ClausesCited,
    bool DriftDetected,
    string? DriftDescription,
    bool IsSuccess,
    string? Error,
    DriftResolution? Resolution = null,
    string? ReconciliationReason = null);

/// <summary>
/// An S04 session. The boundary reuses the same retry/error policy as every
/// other workflow, so provider failures become deterministic results rather
/// than escaping as unhandled exceptions.
/// </summary>
public sealed class SpecAnchoredSession
{
    private readonly object _gate = new();
    private readonly List<TurnRecord> _turns = [];
    private readonly List<SpecClause> _activeSpec;
    private readonly DeterministicAiBoundary<string> _boundary;
    private readonly ILogger<SpecAnchoredSession> _logger;
    private readonly ISpecDriftDetector _detector;

    public SpecAnchoredSession(
        IReadOnlyList<SpecClause> specification,
        DeterministicAiBoundary<string> boundary,
        ILogger<SpecAnchoredSession> logger,
        ISpecDriftDetector? driftDetector = null)
    {
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(boundary);
        ArgumentNullException.ThrowIfNull(logger);

        _activeSpec = specification.ToList();
        _boundary = boundary;
        _logger = logger;
        _detector = driftDetector ?? new KeywordSpecDriftDetector();
    }

    public IReadOnlyList<TurnRecord> Turns
    {
        get { lock (_gate) return _turns.ToList().AsReadOnly(); }
    }

    public IReadOnlyList<SpecClause> ActiveSpecification
    {
        get { lock (_gate) return _activeSpec.ToList().AsReadOnly(); }
    }

    public async Task<TurnRecord> RunTurnAsync(string userInput, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userInput);

        SpecClause[] activeSpec;
        lock (_gate)
            activeSpec = _activeSpec.ToArray();

        var specText = string.Join(
            "\n", activeSpec.Select(c => $"[{c.Id}] {c.Statement}"));

        var systemPrompt =
            "You are working through a multi-step engineering task. " +
            "The following specification clauses are AUTHORITATIVE. " +
            "Every response must be consistent with them. If your response " +
            "would contradict a clause, state the contradiction explicitly " +
            "rather than silently overriding it.\n\n" +
            $"Specification:\n{specText}";

        var boundaryResult = await _boundary.ExecuteAsync(systemPrompt, userInput, ct);

        lock (_gate)
        {
            if (!boundaryResult.IsSuccess)
            {
                var failed = new TurnRecord(
                    TurnNumber: _turns.Count + 1,
                    UserInput: userInput,
                    AiOutput: string.Empty,
                    ClausesCited: [],
                    DriftDetected: false,
                    DriftDescription: null,
                    IsSuccess: false,
                    Error: boundaryResult.Error);

                _turns.Add(failed);
                _logger.LogWarning(
                    "Spec session turn {Turn} failed: {Error}",
                    failed.TurnNumber, failed.Error);
                return failed;
            }

            var output = boundaryResult.Value!;
            var check = _detector.Check(output, activeSpec);

            var cited = activeSpec
                .Where(c => output.Contains(c.Id, StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Id)
                .ToList();

            var turn = new TurnRecord(
                TurnNumber: _turns.Count + 1,
                UserInput: userInput,
                AiOutput: output,
                ClausesCited: cited,
                DriftDetected: check.Detected,
                DriftDescription: check.Description,
                IsSuccess: true,
                Error: null);

            _turns.Add(turn);

            if (check.Detected)
                _logger.LogWarning(
                    "Potential spec drift at turn {Turn}: {Reason}",
                    turn.TurnNumber, check.Description);

            return turn;
        }
    }

    /// <summary>
    /// The only sanctioned ways a specification changes mid-session, plus
    /// explicit correction/decomposition. Never silent, always logged.
    /// </summary>
    public void ReviseSpecification(SpecClause revisedClause, string reason)
    {
        ArgumentNullException.ThrowIfNull(revisedClause);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        lock (_gate)
        {
            var index = _activeSpec.FindIndex(c => c.Id == revisedClause.Id);
            if (index >= 0)
                _activeSpec[index] = revisedClause;
            else
                _activeSpec.Add(revisedClause);

            _logger.LogInformation(
                "Specification clause {Id} revised. Reason: {Reason}",
                revisedClause.Id, reason);
        }
    }

    /// <summary>
    /// Records the resolution of a detected drift. ReviseSpecification
    /// additionally applies the supplied clause revision.
    /// </summary>
    public void ReconcileDrift(
        int turnNumber,
        DriftResolution resolution,
        string reason,
        SpecClause? revisedClause = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        lock (_gate)
        {
            var index = _turns.FindIndex(t => t.TurnNumber == turnNumber);
            if (index < 0)
                throw new ArgumentException(
                    $"No turn {turnNumber} in this session.");

            if (!_turns[index].DriftDetected)
                throw new InvalidOperationException(
                    $"Turn {turnNumber} has no drift to reconcile.");

            if (_turns[index].Resolution is not null)
                throw new InvalidOperationException(
                    $"Turn {turnNumber} has already been reconciled.");

            if (resolution is DriftResolution.ReviseSpecification)
            {
                if (revisedClause is null)
                    throw new ArgumentException(
                        "A revised clause is required for ReviseSpecification.");

                ReviseSpecification(revisedClause, reason);
            }

            _turns[index] = _turns[index] with
            {
                Resolution = resolution,
                ReconciliationReason = reason,
            };

            _logger.LogInformation(
                "Drift at turn {Turn} reconciled by {Resolution}. Reason: {Reason}",
                turnNumber, resolution, reason);
        }
    }
}
