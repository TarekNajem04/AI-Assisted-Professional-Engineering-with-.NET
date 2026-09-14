# Assumptions Made Explicit: A Payment-Service Case Study in Architectural Judgment

[Medium])(https://tareknajem04.medium.com/ai-assisted-professional-engineering-with-net-18-e741cf8001a9)
[LinkedIn](https://www.linkedin.com/pulse/ai-assisted-professional-engineering-net-tarek-najem-er22e)

*The chapter's S01 code is correct in isolation and wrong in its assumptions. The sample is what that same service looks like after an engineer asks every question the original never asked — and each answer is visible in the code.*

## The Engineering Problem

Chapter 1 opens with code that passes review: a payment processor whose every individual decision is defensible. Its defect is not in any line but in what no line states — that the idempotency check assumes a fast cache, that the gateway call assumes no timeout budget, that the write path assumes a crash cannot land between two stores. The failure the chapter documents is precise: the record is persisted but never cached, so the idempotency check on retry misses, and the customer is charged twice. Nothing in the code is wrong. Everything around the code is unexamined.

That is the engineering problem this sample exists to examine: the gap between code that is correct against its specification and code that is correct against its operating conditions. The chapter argues the gap is where architectural judgment lives. The sample makes the judgment tangible by implementing both sides — the assumption left implicit costs a double charge; the assumption made explicit costs a few lines and a comment stating why.

## The Chapter's Question, Concretely

The chapter's arc asks one question in six forms: what does the engineer know that the code, the tests, and the tools do not? Section 1 names the hidden assumptions. Section 2 shows why specification-based verification cannot catch them. Section 3 reframes the work as asking architecture-centric questions before coding. Section 4 ties tool value to the judgment that asks them. Section 5 draws the responsibility boundary at understanding. Section 6 shows the skills behind the boundary compounding over time.

The sample answers with a single service in which each of those moves has a file address. `PaymentProcessor.cs` is the corrected S01 processor. `PreferencesEndpoint.cs` is the S03 question list turned into budgets. `ExternalGatewayClient.cs` carries the S04 engineer-adjusted parameters — retry reduced to 2 with 429 handled, breaker threshold lowered from the AI default of 10 to 3 for actual traffic, timeout set beneath the service's own SLA. `InMemoryOutbox.cs` states the S05 obligations. `DataProcessingWorker.cs` applies the S06 runtime knowledge that eliminates 50,000 closure allocations per second. Reading the sample after the chapter feels less like reading new material than like watching the chapter's argument execute.

## How the Sample Approaches the Problem

The design rule is uniform across all five components: no assumption travels without documentation, and no documentation travels without enforcement. A timeout budget appears twice — once as a named constant with a comment explaining the arithmetic behind it, once as behavior a test or a live call can observe. The idempotency guarantee appears three times — as a contract method whose signature makes two writes one call, as a lock in the adapter that honours the signature, and as a test that persists both artifacts through a single operation. This triple appearance is deliberate. A claim stated only in comments is a wish; a claim stated in the contract, honoured by the adapter, and verified by a test is engineering.

The composition root follows the same rule. `Program.cs` is the only file that knows all layers, and it says so. The in-memory adapters are singletons, with a comment explaining that scoped instances would lose idempotency state between requests — the exact S01 failure, reintroduced by a lifetime choice. The demo seed is marked as sample-only, not a production pattern. Nothing about the assembly is clever; everything about it is legible, which is the point the chapter makes about architecture-centric work generally.

## Key Engineering Decisions

**One owner per contract.** The application ports exist exactly once, in `Application.Ports`. An earlier state of this sample declared the same interfaces in three namespaces, and the result was instructive: the application compiled, all tests passed, and the service crashed on startup because consumers and implementations had bound to different copies. The repair — deleting the twins rather than adapting to them — is itself the chapter's lesson in miniature. Duplicated contracts are duplicated assumptions about who owns the truth.

**Atomicity by signature, not by comment.** The repository contract exposes a single `SaveAsync(payment, idempotencyKey)` call instead of separate save and record methods. The previous shape allowed the crash window the chapter diagnoses; no comment could close it, because the window lived in the call sequence, not in either call. Moving the guarantee into the signature is the S01 fix made structural: the contract now makes the incorrect usage inexpressible rather than merely discouraged.

**Deterministic stand-ins, honestly labelled.** The stub gateway selects behaviour by token prefix — success, decline, ten-second hang — so every documented outcome is reproducible without a provider account. The file states what the stub proves (processor logic) and what it cannot prove (provider behaviour). The outbox adapter is equally explicit: its lock makes one append indivisible but does not replicate a business-row-plus-event-row transaction, and its durability is process memory. A sample that confessed less would be easier to admire and impossible to trust.

**Typed failures at every boundary.** Gateway timeout and gateway unreachable both return 503 with distinct codes; declines return 400 with the provider's code; duplicates return 200 without touching the gateway. The mapping encodes the S03 discipline that a failure's type is information: 503 tells the caller to retry, 400 tells the caller to fix the request, and 500 is reserved for faults that are genuinely this service's own. During verification, the unreachable path was observed live against a non-existent host — 503 in about a second, no exception escaping.

## What the Tests Actually Demonstrate

Nine unit tests prove isolated domain behaviour: creation invariants, currency normalisation, state-transition legality. They prove nothing about the system, and the sample does not ask them to. Nine integration tests prove processor and outbox logic against shaped in-memory doubles — including the real four-second timeout path and the crash-before-mark redelivery that defines at-least-once semantics. The concurrent-store test proves the adapter's lock holds under fifty parallel writes. What none of these prove is production behaviour, and the suite's honesty about that is its most chapter-faithful property: the S02 lesson is that tests verify specified behaviour, so the sample's runtime smoke transcript — 201, duplicate 200, decline 400, timeout 503, degraded-preferences 200, worker batch 3/3 — is recorded separately as observed behaviour, not inferred from green tests.

## What the Sample Does Not Demonstrate

The outbox has no hosted dispatcher; dispatch is proven by tests, not by traffic, and the README says so. There is no real Redis, database, or provider — the preferences degradation and the Polly pipeline are verified against absence and stubs, never against the systems they name. The resilience parameters match the chapter's engineer-adjusted values, but no claim is made that they are tuned: tuning requires the production traffic they were chosen for. Scalability, availability, and security are not demonstrated and are not claimed. The sample is a controlled treatment of one concern — making assumptions explicit — and its boundaries are part of the treatment.

## Boundaries of the Argument

- The case-study reading covers the sample as published: eighteen tests, five demonstrated chapter concepts, one composition root. Its evidence includes verification performed during the sample's publication review; it does not narrate that review's process.
- The claim that contracts should have exactly one owner generalises beyond this codebase; the claim that ports belong in the application layer is this sample's architecture, not a universal law.
- The stub-gateway technique demonstrates processor logic deterministically; it says nothing about provider behaviour, and the article must not be read as endorsing stubs as provider verification.
- The outbox discussion demonstrates the responsibility boundary's three obligations as a contract shape; a durable implementation remains future work by design, not by oversight.

## The Sample

`DistributedPaymentService` — the Chapter 1 sample of *AI-Assisted Professional Engineering with .NET* — implements the chapter's engineering argument as a runnable .NET 8 service: four source projects in clean layering, two test projects with eighteen tests, a composition root that states its own reasoning, and documentation that confesses every simplification. It is published as repository content on `main`, with no independent release, in the canonical `book/samples/Chapter-01/DistributedPaymentService` location.

If you have ever read code that was obviously correct and felt uneasy without knowing why, you have met the gap this sample is built to close. The unease was architectural judgment without a method. The sample is the method, worked end to end on one service. The question Chapter 02 takes up is what sits on the other side of the boundary the sample defends: how the AI systems themselves work, where they fail, and which mental models of them survive contact with production.

---

## Engineering Series

Previous

[**← 017-The Engineering Transformation: Reading Chapter 1 as One Argument**](../../v0.1.6/017-The-Engineering-Transformation/article.en.md)

---

## Continue the Journey

This essay is drawn from the **Chapter 1 sample** of *AI-Assisted Professional Engineering with .NET*. The sample contains the full service, both test suites, and the documented verification transcript.

- **GitHub Repository:** <https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET>
- **Sample Source:** <https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET/tree/main/book/samples/Chapter-01/DistributedPaymentService>

---

*In this case study, we traced how one sample makes the chapter's central move five times over — turning hidden assumptions about idempotency, budgets, resilience parameters, publication obligations, and runtime allocation into contracts, adapters, and verified behaviour. Chapter 02 examines the AI systems on the far side of that work: how they behave, where they break, and how to reason about them accurately.*
