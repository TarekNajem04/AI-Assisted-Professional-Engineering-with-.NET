# Engineering Communication Toolkit — Chapter 03 Sample

This sample is the runnable companion to **Chapter 03 — Engineering Communication with AI**. It demonstrates how probabilistic model calls become manageable when the surrounding engineering artifacts — contracts, context, workflows, specifications, schemas, and session state — are explicit, typed, and owned by the application.

## What it demonstrates

| Chapter section | Sample implementation |
|---|---|
| S01 — versioned prompt contract | `src/AiToolkit.Core/Prompts/PromptContract.cs` |
| S02 — budgeted context assembly | `src/AiToolkit.Infrastructure/AI/ContextBuilder.cs` |
| S03 — reusable DI workflow + deterministic boundary | `IAiWorkflow.cs`, `WorkflowRegistry.cs` |
| S04 — specification-anchored session | `SpecAnchoredSession.cs`, including drift reconciliation |
| S05 — variance reduction | `SchemaConstrainedClient.cs`, wired into the registered review workflow |
| S06 — explicit multi-step state and approval gate | `MultiStepAiSession.cs` |

The sample deliberately **does not** demonstrate durable persistence, distributed concurrency, or a production-ready drift classifier. Session stores are in-memory dictionaries, and the drift detector is a deterministic keyword-based seam that can be replaced with a dedicated model check.

## Prerequisites

- .NET 8 SDK (the sample targets `net8.0`).
- For real generation: an OpenAI API key placed in `OPENAI_API_KEY`. No key is stored in the repository.
- For deterministic verification: no key is required. Use the built-in fake client mode described below.

## Build and test

From this directory:

```powershell
dotnet restore EngineeringCommunicationToolkit.slnx
dotnet build EngineeringCommunicationToolkit.slnx --no-restore
dotnet test EngineeringCommunicationToolkit.slnx --no-build
dotnet format EngineeringCommunicationToolkit.slnx --verify-no-changes --no-restore
```

Expected result: build succeeds with **0 warnings and 0 errors**, all tests pass, and the format check exits `0`.

## Run with deterministic fake clients

This is the recommended way to verify every sample path without credentials or network access.

```powershell
$env:AiToolkit__ClientMode = 'Fake'
$env:AiToolkit__FakeFreeFormOutput = 'C01: the output stays consistent with the clause.'
$env:AiToolkit__FakeStructuredOutput = '{"issues":[{"line":1,"severity":"warning","description":"Sample finding"}]}'
dotnet run --project src/AiToolkit.Api --urls http://127.0.0.1:5082
```

To verify provider-failure handling, set `$env:AiToolkit__FakeFailure = 'true'` before starting the API. Requests then return structured `422` responses instead of unhandled exceptions.

## Run with a real model

```powershell
$env:OPENAI_API_KEY = '<your key>'
Remove-Item Env:AiToolkit__ClientMode -ErrorAction SilentlyContinue
dotnet run --project src/AiToolkit.Api --urls http://127.0.0.1:5082
```

The model is fixed to `gpt-4o-mini` to keep the sample reproducible. Real model output is nondeterministic; the fake mode is the basis for the sample's deterministic verification.

## Endpoints

| Method | Path | Purpose |
|---|---|---|
| GET | `/health` | Liveness probe |
| GET | `/workflows` | Lists registered workflow IDs |
| POST | `/workflows/review` | Runs the schema-constrained code-review workflow |
| POST | `/sessions` | Starts a specification-anchored session |
| POST | `/sessions/{id}/turn` | Runs one turn and reports drift/citations |
| POST | `/sessions/{id}/turns/{n}/reconcile` | Records an explicit drift resolution |
| POST | `/tasks` | Runs a two-step task with an explicit approval identity |

Example requests:

```powershell
Invoke-RestMethod http://127.0.0.1:5082/health
# {"status":"healthy"}

Invoke-RestMethod http://127.0.0.1:5082/workflows
# {"workflows":["code-review@1.0"]}

$session = Invoke-RestMethod -Method Post http://127.0.0.1:5082/sessions `
  -ContentType application/json `
  -Body '{"specClauses":["C1 output must cite a file","C2 output must state risk"]}'

Invoke-RestMethod -Method Post "http://127.0.0.1:5082/sessions/$($session.sessionId)/turn" `
  -ContentType application/json `
  -Body '{"userInput":"Review the change"}'

Invoke-RestMethod -Method Post http://127.0.0.1:5082/tasks `
  -ContentType application/json `
  -Body '{"fileName":"Demo.cs","language":"csharp","code":"var x = 1;","approvedBy":"engineer@example.com"}'
```

The `approvedBy` field is required. The sample never invents human approval; the caller supplies the identity that represents the engineer's decision.

To exercise drift and reconciliation deterministically, start with a contradictory fake output:

```powershell
$env:AiToolkit__FakeFreeFormOutput = 'C01 violates C02 by ignoring clause C02.'
# restart the API, create a session, run a turn, then:
Invoke-RestMethod -Method Post "http://127.0.0.1:5082/sessions/$($session.sessionId)/turns/1/reconcile" `
  -ContentType application/json `
  -Body '{"resolution":"ReviseSpecification","reason":"Clause C02 was ambiguous","revisedClause":{"id":"C02","statement":"Output must state one concrete risk."}}'
```

Invalid inputs produce `400`, unknown sessions produce `404`, and workflow/provider failures produce `422` with an `error` field.

## Verification status

The final pre-publication verification was executed from this directory. Restore, build, format check, tests, API startup, endpoint behavior, negative cases, and fake-client AI paths all passed.

`VERDICT: PASS`
