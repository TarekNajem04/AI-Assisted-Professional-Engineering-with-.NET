<!-- book/samples/Chapter-02/UnderstandingLLMs/README.md -->
# AiAwareEngineeringToolkit
## Chapter 02 Sample Project — Understanding Large Language Models for Engineers

### Purpose

This sample demonstrates the mechanical concepts from Chapter 02 in a
production-realistic .NET 8 / C# 14 toolkit. Every file maps to a specific
section's concept, with inline comments explaining the connection.

### Concepts Demonstrated

| File | Chapter Concept |
|---|---|
| `Core/Tokens/TokenEstimator.cs` | S01 — token cost estimation primitive |
| `Infrastructure/AI/StreamingCompletionClient.cs` | S01 — per-token streaming, sequential generation |
| `Infrastructure/AI/AiSuggestionValidator.cs` | S04 — fabrication/outdated-pattern detection |
| `Infrastructure/AI/DeterministicAiBoundary.cs` | S05 — validate/retry deterministic seam |
| `Infrastructure/AI/VerificationLevel.cs` | S06 — calibrated trust levels |
| `Infrastructure/AI/AiGeneratedAttribute.cs` | S06 — per-artifact verification annotation |
| `Infrastructure/AI/AiAuditLog.cs` | S06 — audit trail for AI-generated components |
| `Api/Endpoints/AiSuggestionEndpoint.cs` | S04–S06 — wires validator + boundary + audit into one endpoint |

### Architecture

```
AiToolkit.Core            ← Pure calculation, no framework dependency (TokenEstimator)
AiToolkit.Application     ← Port interfaces (IAiOutputParser, IChatCompletionClient)
AiToolkit.Infrastructure  ← Implementations: validator, boundary, audit, streaming client
AiToolkit.Api             ← Minimal API endpoint, DI composition root
```

This follows the same Core/Application/Infrastructure/Api layering as the
Chapter 01 sample (`DistributedPaymentService`) for architectural
consistency across the book, even though this project is conceptually
standalone rather than a continuation of the Chapter 01 domain.

### Running the Project

```bash
cd src/AiToolkit.Api
dotnet run
```

Requires `OPENAI_API_KEY` environment variable for the streaming client
to function; the validator, boundary, and audit log components work
fully offline against any `IChatCompletionClient` stub for testing.

> Missing-key behavior: `appsettings.json` ships with an empty `ApiKey`,
> so without the environment variable the API still starts, but the first
> call to `POST /ai/suggestions` fails fast with a clear
> `InvalidOperationException` instead of a bare 500. The Development
> exception page shows stack traces — do not deploy this sample as-is
> without adding `UseExceptionHandler` (or equivalent) first.

### Running the Tests

```bash
dotnet test tests/Unit/AiToolkit.Tests.Unit.csproj
```
