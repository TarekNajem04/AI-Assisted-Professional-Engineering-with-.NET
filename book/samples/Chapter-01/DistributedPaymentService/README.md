<!-- book/samples/Chapter-01/DistributedPaymentService/README.md -->
# DistributedPaymentService
## Chapter 01 Sample Project — The Engineering Transformation

### Purpose

This sample demonstrates the architectural principles from Chapter 01 in a
production-realistic .NET 8 service. Every design decision maps to
a concept from the chapter.

Prerequisites: .NET 8 SDK or later. No Redis required — the preferences
endpoint degrades to source-of-truth reads when no cache is reachable.

### Concepts Demonstrated

| File | Chapter Concept |
|---|---|
| `PaymentProcessor.cs` | S01 — hidden operational assumptions made explicit |
| `PreferencesEndpoint.cs` | S03 — architecture-centric vs implementation-centric |
| `ExternalGatewayClient.cs` | S04 — AI-assisted Polly, engineer-adjusted for real traffic |
| `InMemoryOutbox.cs` (`Ports/IOutbox.cs`) | S05 — responsibility boundary: outbox with 3 obligations stated |
| `DataProcessingWorker.cs` | S06 — CLR runtime knowledge: closure allocation fix |

### Running the Project

```bash
cd book/samples/Chapter-01/DistributedPaymentService
dotnet run --project src/PaymentService.Api/PaymentService.Api.csproj
```

### Verifying It Works

With the app running (default `http://localhost:5208`):

```bash
# 1. Process a payment → 201 Created with the payment id
curl -X POST http://localhost:5208/payments \
  -H "Content-Type: application/json" \
  -d '{"idempotencyKey":"demo-key-1","customerId":"3F2504E0-4F89-11D3-9A0C-0305E82C3301","amount":{"value":100,"currency":"USD"},"method":{"type":"card","token":"tok_test"}}'
# → 201 {"id":"e00d6558-2760-4c81-b02d-2201989468f3"}

# 2. Repeat the identical request → 200 {"status":"duplicate"} (no double charge)
# 3. Declined card → 400 {"error":"CARD_DECLINED"} (token "tok_decline_*")
# 4. Hanging provider → 503, ~4 s (token "tok_timeout_*", typed gateway timeout)
# 5. Preferences without Redis → 200 from source of truth (degraded, not error):
curl http://localhost:5208/users/demo-user/preferences
# → 200 {"userId":"demo-user","currency":"USD","language":"en","emailNotifications":true}
```

On startup the console shows the seeded worker batches processed and
acknowledged (`Batch batch-0001 processed: 3/3 succeeded`).

Token prefixes select the stub gateway path (`tok_timeout_*`, `tok_decline_*`,
anything else succeeds). Set `ExternalGateway:UseStub=false` plus a real
`ExternalGateway:BaseUrl` for the Polly-pipelined live client.

### Tests

```bash
dotnet test DistributedPaymentService.slnx
```

Unit (9) + Integration (4 processor + 5 outbox) = 18 tests. The integration
doubles are in-memory and shaped like the durable adapters — they verify
processor and outbox logic, not production infrastructure.

The outbox is registered in DI but has no hosted dispatcher in this sample:
dispatch (store → read → mark, redelivery, depth) is demonstrated and
verified by the outbox integration tests, not by runtime traffic.

### Architecture

```
PaymentService.Core          ← Domain models, no framework dependency
PaymentService.Application   ← Use cases, resilience orchestration
PaymentService.Infrastructure← External integrations (gateway, cache, db)
PaymentService.Api           ← Minimal API endpoints, DI composition
```

### Key Engineering Decisions

All decisions are documented inline with the reasoning that an
architecture-centric engineer would apply before writing a single line.

Timeout budgets, CancellationToken propagation, cache TTL jitter,
circuit breaker thresholds calibrated to actual traffic patterns —
every parameter is explained, not assumed.
