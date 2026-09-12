# DistributedPaymentService — Project Structure

Chapter 01 sample for *AI-Assisted Professional Engineering with .NET*.
This file lists every file in the sample. Update it when files are added or removed.

```
DistributedPaymentService/
├── DistributedPaymentService.slnx
├── README.md                        ← purpose, concept map, run instructions
├── PROJECT_STRUCTURE.md             ← this file
├── .editorconfig
├── src/
│   ├── PaymentService.Core/
│   │   ├── PaymentService.Core.csproj
│   │   └── Domain/
│   │       └── Payment.cs           ← aggregate root, value objects, PaymentMethod
│   ├── PaymentService.Application/
│   │   ├── PaymentService.Application.csproj
│   │   ├── ApplicationExtensions.cs ← AddApplication() (processor registration)
│   │   ├── Ports/                   ← the ONLY declarations of shared contracts
│   │   │   ├── IPaymentGateway.cs   ← + GatewayResponse
│   │   │   ├── IPaymentRepository.cs← + FindByIdAsync
│   │   │   ├── IMessageBus.cs       ← + IDataProcessor, Message, MessageBatch
│   │   │   ├── IUserPreferenceService.cs ← + UserPreferences
│   │   │   └── IOutbox.cs           ← S05 outbox contract + 3 obligations
│   │   └── UseCases/
│   │       └── PaymentProcessor.cs  ← S01 explicit-assumption processor
│   ├── PaymentService.Infrastructure/
│   │   ├── PaymentService.Infrastructure.csproj
│   │   ├── GlobalUsings.cs
│   │   ├── Gateway/
│   │   │   ├── ExternalGatewayClient.cs ← S04 engineer-adjusted Polly pipeline
│   │   │   └── StubGatewayClient.cs     ← deterministic test-double gateway
│   │   ├── Messaging/
│   │   │   ├── InMemoryMessageBus.cs    ← bus adapter (thread-safe acks)
│   │   │   └── InMemoryOutbox.cs        ← S05 at-least-once outbox adapter
│   │   ├── Processing/
│   │   │   └── LoggingDataProcessor.cs
│   │   ├── Repositories/
│   │   │   └── InMemoryPaymentRepository.cs
│   │   └── Services/
│   │       └── InMemoryUserPreferenceService.cs
│   └── PaymentService.Api/
│       ├── PaymentService.Api.csproj
│       ├── PaymentService.Api.csproj.user ← IDE state, never commit
│       ├── Program.cs                   ← composition root + demo seed
│       ├── GlobalUsings.cs
│       ├── appsettings.json / appsettings.Development.json
│       ├── Properties/launchSettings.json
│       ├── Endpoints/
│       │   └── PreferencesEndpoint.cs   ← S03 budgeted endpoint
│       ├── Serialization/
│       │   └── ValueObjectConverters.cs ← CustomerId/PaymentId JSON converters
│       └── Workers/
│           └── DataProcessingWorker.cs   ← S06 closure-free batch loop
└── tests/
    ├── Unit/
    │   ├── PaymentService.Tests.Unit.csproj
    │   └── Domain/PaymentTests.cs       ← 9 domain tests
    └── Integration/
        ├── PaymentService.Tests.Integration.csproj
        ├── PaymentProcessorIntegrationTests.cs ← 4 processor tests (in-memory doubles)
        └── OutboxTests.cs               ← 5 outbox contract tests
```

## Layering rule

```
Api → Application → Core
Api → Infrastructure → { Application, Core }
```

`PaymentService.Core` has no outward dependencies. Shared contracts live
exactly once, in `Application.Ports`. The Api project is the composition
root — the only place that knows all layers.

## Naming conventions

| Artifact | Convention | Example |
|---|---|---|
| Sample folder | `Chapter-NN/SolutionName` | `Chapter-01/DistributedPaymentService` |
| Source project | `PaymentService.<Layer>` | `PaymentService.Application` |
| Test project | `PaymentService.Tests.<Scope>` | `PaymentService.Tests.Integration` |
| Path comment | First line of every source file | `// book/samples/...` |
