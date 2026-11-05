// Wires Sections 04, 05, and 06 into a single Minimal API endpoint:
//   S04 — AiSuggestionValidator catches fabrication/outdated patterns
//   S05 — DeterministicAiBoundary enforces the deterministic seam
//         (validate → retry-with-feedback → typed result)
//   S06 — every accepted suggestion is annotated with a VerificationLevel
//         and recorded in AiAuditLog for later review/incident analysis

using AiToolkit.Infrastructure.AI;

namespace AiToolkit.Api.Endpoints;

public static class AiSuggestionEndpoint {
    public static void MapAiSuggestionEndpoints(this WebApplication app) {
        app.MapPost("/ai/suggestions", async (
                SuggestionRequest request,
                DeterministicAiBoundary<CodeSuggestion> boundary,
                AiAuditLog auditLog,
                ILogger<Program> logger,
                CancellationToken ct) => {
                    const string systemPrompt =
                        """
                        You are a senior .NET engineer.
                        Provide a single fenced C# code block followed by a brief explanation.
                        If you are not confident an API exists exactly as described, say so.
                        """;

                    // S05: the probabilistic call happens entirely inside the
                    // boundary. This endpoint only ever sees a typed result.
                    var result = await boundary.ExecuteAsync(
                        systemPrompt, request.Prompt, ct);

                    if(!result.IsSuccess) {
                        logger.LogWarning(
                            "AI suggestion request failed validation: {Error}",
                            result.Error);

                        // S04: validation failure after all retries is a typed,
                        // explicit failure — not a silently-accepted hallucination.
                        return Results.UnprocessableEntity(new {
                            error = "AI suggestion failed validation",
                            detail = result.Error,
                        });
                    }

                    // S06: record the verification level applied. This sample
                    // applies Moderate by default — structural + semantic
                    // validation via the boundary, no deep architectural review
                    // (which would require a human reviewer, not this endpoint).
                    auditLog.Record(new AiAuditEntry(
                        ComponentId: $"suggestion-{Guid.NewGuid():N}",
                        VerificationLevel: VerificationLevel.Moderate,
                        Notes: "Validated via AiSuggestionValidator " +
                                           "(namespace fabrication + outdated " +
                                           "pattern checks) inside DeterministicAiBoundary.",
                        AcceptedAt: DateTimeOffset.UtcNow,
                        AcceptedBy: "AiSuggestionEndpoint"));

                    return Results.Ok(new SuggestionResponse(
                        Code: result.Value!.Code,
                        Explanation: result.Value.Explanation,
                        VerificationLevel: VerificationLevel.Moderate.ToString()));
                })
            .WithName("GetAiSuggestion")
            .Produces<SuggestionResponse>()
            .Produces(422);

        // S06: expose the audit log for review tooling / incident analysis.
        app.MapGet("/ai/audit", (AiAuditLog auditLog) =>
                Results.Ok(auditLog.Entries))
            .WithName("GetAiAuditLog");
    }
}

public sealed record SuggestionRequest(string Prompt);

public sealed record SuggestionResponse(
    string Code,
    string Explanation,
    string VerificationLevel);
