// book/samples/Chapter-03/EngineeringCommunicationToolkit/src/AiToolkit.Api/Endpoints/WorkflowEndpoint.cs
//
// Wires all six section concepts into a cohesive Minimal API surface:
//   GET  /health                                      — liveness
//   GET  /workflows                                   — registered workflow IDs (S03)
//   POST /workflows/review                            — code review (S01-S03, S05)
//   POST /sessions                                    — start a spec-anchored session (S04)
//   POST /sessions/{id}/turn                          — run one session turn (S04)
//   POST /sessions/{id}/turns/{n}/reconcile           — reconcile detected drift (S04)
//   POST /tasks                                       — multi-step task (S05-S06)

using System.Collections.Concurrent;
using AiToolkit.Application.Ports;
using AiToolkit.Core.Prompts;
using AiToolkit.Infrastructure.AI;

namespace AiToolkit.Api.Endpoints;

public static class WorkflowEndpoint
{
    // In-memory stores keep the sample self-contained. MultiStepAiSession's
    // CompletedSteps would serialize naturally into a durable store in production.
    private static readonly ConcurrentDictionary<string, SpecAnchoredSession> Sessions = new();
    private static readonly ConcurrentDictionary<string, MultiStepAiSession> Tasks = new();

    public static void MapWorkflowEndpoints(this WebApplication app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
            .WithName("Health");

        // ── S03: workflow discovery ───────────────────────────────────────
        app.MapGet("/workflows", (
            IAiWorkflow<CodeReviewRequest, string> reviewWorkflow) =>
            Results.Ok(new { workflows = new[] { reviewWorkflow.WorkflowId } }))
            .WithName("ListWorkflows");

        // ── S01-S03 + S05: structured code review ─────────────────────────
        app.MapPost("/workflows/review", async (
            ReviewRequest request,
            IAiWorkflow<CodeReviewRequest, string> workflow,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.FileName) ||
                string.IsNullOrWhiteSpace(request.Language) ||
                request.Code is null)
            {
                return Results.BadRequest(new
                {
                    error = "fileName, language and code are required.",
                });
            }

            var input = new CodeReviewRequest(
                FileName: request.FileName,
                Language: request.Language,
                Code: request.Code,
                FocusAreas: request.FocusAreas ?? ["correctness", "performance"]);

            var result = await workflow.ExecuteAsync(input, ct);

            return result.IsSuccess
                ? Results.Ok(new
                {
                    review = result.Value,
                    workflowId = result.WorkflowId,
                    contractVersion = result.ContractVersion,
                })
                : Results.UnprocessableEntity(new { error = result.Error });
        }).WithName("ReviewCode");

        // ── S04: start specification-anchored session ─────────────────────
        app.MapPost("/sessions", (
            StartSessionRequest request,
            IAiClientFactory factory,
            IAiOutputParser<string> parser,
            ISpecDriftDetector driftDetector,
            ILoggerFactory loggerFactory) =>
        {
            if (request.SpecClauses is null ||
                request.SpecClauses.Count == 0 ||
                request.SpecClauses.Any(string.IsNullOrWhiteSpace))
            {
                return Results.BadRequest(new
                {
                    error = "specClauses must contain at least one non-empty clause.",
                });
            }

            var clauses = request.SpecClauses
                .Select((s, i) => new SpecClause($"C{i + 1:00}", s))
                .ToList();

            var boundary = new DeterministicAiBoundary<string>(
                chatClient: factory.CreateFreeForm(),
                parser: parser,
                logger: loggerFactory.CreateLogger<DeterministicAiBoundary<string>>());

            var session = new SpecAnchoredSession(
                specification: clauses,
                boundary: boundary,
                logger: loggerFactory.CreateLogger<SpecAnchoredSession>(),
                driftDetector: driftDetector);

            var id = Guid.NewGuid().ToString("N")[..8];
            Sessions[id] = session;

            return Results.Ok(new { sessionId = id, clauses = clauses.Count });
        }).WithName("StartSession");

        // ── S04: run one turn of a spec-anchored session ──────────────────
        app.MapPost("/sessions/{id}/turn", async (
            string id, TurnRequest request, CancellationToken ct) =>
        {
            if (!Sessions.TryGetValue(id, out var session))
                return Results.NotFound(new { error = $"Session '{id}' not found." });

            var turn = await session.RunTurnAsync(request.UserInput ?? string.Empty, ct);

            if (!turn.IsSuccess)
                return Results.UnprocessableEntity(new { error = turn.Error });

            return Results.Ok(new
            {
                turnNumber = turn.TurnNumber,
                aiOutput = turn.AiOutput,
                driftDetected = turn.DriftDetected,
                driftDescription = turn.DriftDescription,
                clausesCited = turn.ClausesCited,
                resolution = turn.Resolution?.ToString(),
                reconciliationReason = turn.ReconciliationReason,
            });
        }).WithName("RunSessionTurn");

        // ── S04: explicit reconciliation of detected drift ────────────────
        app.MapPost("/sessions/{id}/turns/{turnNumber:int}/reconcile", (
            string id, int turnNumber, ReconcileRequest request) =>
        {
            if (!Sessions.TryGetValue(id, out var session))
                return Results.NotFound(new { error = $"Session '{id}' not found." });

            if (!Enum.TryParse(request.Resolution, true, out DriftResolution resolution))
            {
                return Results.BadRequest(new
                {
                    error = "resolution must be CorrectOutput, ReviseSpecification or DecomposeClause.",
                });
            }

            var revisedClause = request.RevisedClause is null
                ? null
                : new SpecClause(request.RevisedClause.Id, request.RevisedClause.Statement);

            try
            {
                session.ReconcileDrift(turnNumber, resolution, request.Reason, revisedClause);
                var turn = session.Turns.Single(t => t.TurnNumber == turnNumber);

                return Results.Ok(new
                {
                    turnNumber,
                    resolution = turn.Resolution?.ToString(),
                    reason = turn.ReconciliationReason,
                    clauses = session.ActiveSpecification.Select(c => c.Id),
                });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.UnprocessableEntity(new { error = ex.Message });
            }
        }).WithName("ReconcileSessionDrift");

        // ── S05-S06: run a multi-step AI task ────────────────────────────
        app.MapPost("/tasks", async (
            MultiStepTaskRequest request,
            IAiWorkflow<CodeReviewRequest, string> workflow,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.FileName) ||
                string.IsNullOrWhiteSpace(request.Language) ||
                request.Code is null ||
                string.IsNullOrWhiteSpace(request.ApprovedBy))
            {
                return Results.BadRequest(new
                {
                    error = "fileName, language, code and approvedBy are required. " +
                            "approvedBy represents the explicit human decision.",
                });
            }

            var taskId = Guid.NewGuid().ToString("N")[..8];
            var session = new MultiStepAiSession(
                taskId,
                loggerFactory.CreateLogger<MultiStepAiSession>());

            // Step 1 — Advisor: the caller supplies the approver identity.
            // No approval is inferred by the sample.
            var classifyInput = new CodeReviewRequest(
                FileName: request.FileName,
                Language: request.Language,
                Code: request.Code,
                FocusAreas: ["classify risk level"]);

            var step1 = await session.RunStepAsync(
                stepName: "classify-risk",
                pattern: CollaborationPattern.Advisor,
                workflow: workflow,
                input: classifyInput,
                verificationLevel: VerificationLevel.Moderate,
                ct: ct);

            session.ApproveAdvisorStep(step1.StepNumber, request.ApprovedBy);
            session.EnsureStepApproved(step1.StepNumber);

            // Step 2 — Reviewer: the registered workflow uses the
            // SchemaConstrainedClient (S05) for both steps.
            var reviewInput = new CodeReviewRequest(
                FileName: request.FileName,
                Language: request.Language,
                Code: request.Code,
                FocusAreas: ["correctness", "performance", "security"]);

            var step2 = await session.RunStepAsync(
                stepName: "full-review",
                pattern: CollaborationPattern.Reviewer,
                workflow: workflow,
                input: reviewInput,
                verificationLevel: VerificationLevel.Deep,
                ct: ct);

            Tasks[taskId] = session;

            return Results.Ok(new
            {
                taskId,
                approvedBy = request.ApprovedBy,
                classification = step1.Output,
                review = step2.Output,
                steps = session.CompletedSteps.Select(s => new
                {
                    s.StepNumber,
                    s.StepName,
                    Pattern = s.Pattern.ToString(),
                    Level = s.VerificationLevel.ToString(),
                    s.HumanApproved,
                }),
            });
        }).WithName("RunMultiStepTask");
    }
}

// ── Request / response DTOs ────────────────────────────────────────────────

public sealed record ReviewRequest(
    string? FileName,
    string? Language,
    string? Code,
    IReadOnlyList<string>? FocusAreas);

public sealed record StartSessionRequest(IReadOnlyList<string>? SpecClauses);

public sealed record TurnRequest(string? UserInput);

public sealed record ReconcileRequest(
    string Resolution,
    string Reason,
    RevisedClauseRequest? RevisedClause);

public sealed record RevisedClauseRequest(string Id, string Statement);

public sealed record MultiStepTaskRequest(
    string? FileName,
    string? Language,
    string? Code,
    string? ApprovedBy);
