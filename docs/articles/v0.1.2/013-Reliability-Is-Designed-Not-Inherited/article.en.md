# Reliability Is Designed, Not Inherited

[Medium](https://tareknajem04.medium.com/ai-assisted-professional-engineering-with-net-13-7dc513f664a4)
[LinkedIn](https://www.linkedin.com/pulse/ai-assisted-professional-engineering-net-tarek-najem-t7x5e)

*The architecture-centric frame, and the difference between a correct implementation and a reliable system.*

## The Same Endpoint, Two Systems

Consider an endpoint in an ASP.NET Core service that reads a user's preferences from a downstream service, caches them, and returns them to the caller. A competent engineer writes the straightforward version:

```csharp
app.MapGet("/users/{userId}/preferences", async (
    string userId,
    IUserPreferenceService preferenceService,
    IDistributedCache cache,
    CancellationToken ct) =>
{
    var cached = await cache.GetStringAsync(userId, ct);
    if (cached is not null)
        return Results.Ok(JsonSerializer.Deserialize<UserPreferences>(cached));

    var preferences = await preferenceService.GetAsync(userId, ct);
    await cache.SetStringAsync(userId,
        JsonSerializer.Serialize(preferences),
        new DistributedCacheEntryOptions
            { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) },
        ct);

    return Results.Ok(preferences);
});
```

This is not careless code. It calls the dependency, caches with a sensible TTL, returns the result, and lets the default error handling do its work. Under normal load, with a healthy downstream and a warm cache, it is correct. It compiles, it passes its tests, and a reviewer would approve it.

A second version of the same endpoint makes a series of additional decisions before and during the implementation: a bounded time budget for the cache read, so a slow Redis cannot block the response; a defined timeout for the downstream call, so a degraded service produces a typed `503` instead of an unhandled exception; a write-behind cache update, so a failed write degrades performance rather than correctness; a jittered TTL, so cache entries do not expire in a synchronized wave.

The two implementations are not a good version and a bad version. In a bounded system — low traffic, a reliable downstream, a local cache — the first version will work correctly for years. The second version is not "more defensive" or "more pessimistic"; it is the first version with one difference that matters: **every behavior under a plausible operating state was decided, rather than inherited from the defaults of the APIs involved.**

That difference is the subject of this essay.

## Two Frames, Two Sets of Questions

The distinction is not about the size of the problem being solved. It is about the frame through which the problem is viewed, and the questions that frame generates.

The **implementation-centric frame** asks: *Does this code correctly solve the problem as specified?* Its questions are local and its answers terminate. Does the function return the right value? Do the tests pass? Does the reviewer approve? Does the linter accept it? The frame is closed: it evaluates the code against its specification, and a passing evaluation means the work is done.

The **architecture-centric frame** asks: *How will this code behave in the system under real operational conditions, and what are the consequences of that behavior for the system's overall reliability and evolution?* Its questions are distributed and its answers do not terminate in the same way. What happens to the callers of this service when the external dependency it introduces becomes unavailable? What does the failure of this implementation look like in the distributed trace? If this code changes, which contracts have changed, and who depends on them? The frame is open: it evaluates the code against the system, and a complete evaluation requires knowledge that extends far beyond the file being changed.

This asymmetry in termination conditions is the mechanism behind the strongest bias an engineer faces in adopting architecture-centric thinking. The implementation frame *feels responsible* because it finishes. The architecture frame *feels like an endless obligation* — every decision opens onto the whole system, and the whole system is never fully known. The perception is real, and it is the main reason the shift is resisted even by engineers who accept the argument.

The argument this essay develops is that the perception is wrong in a specific, correctable way: the obligations are not endless. They are a finite set of recurring questions, each of which becomes faster to answer with practice.

## Designed Decisions, Inherited Defaults

It is worth being precise about what the architecture-centric questions are doing, because their effect is easy to mischaracterize.

They are not producing a more defensive implementation. Every code path has behavior in every state — a caller of the first version does not receive nothing when Redis is slow; it receives an indefinite wait, because no timeout was defined. The behavior exists; it was simply never chosen. When the downstream degrades, the caller does not receive a clean signal; it receives an unhandled exception, because no degraded-mode contract was decided. Both implementations "handle" failure in the sense that code runs under every condition. The difference is that the second version's failure behavior was a designed decision, while the first version's was an accident of the defaults.

Call this the distinction between a **designed decision** and an **inherited default**. A designed decision is a behavior chosen deliberately, with its consequences at the system level considered. An inherited default is a behavior that exists because no one chose anything — the API's default timeout, the framework's default error response, the cache's default eviction policy.

Reliability is the cumulative product of these choices being made rather than inherited. A single inherited default is almost never the cause of a production incident by itself. What fails is the accumulation: ten endpoints, each with an undecided timeout; three caching layers, each with an unexamined invalidation; a retry policy added because it was the known pattern, not because the operation was verified idempotent. Each local decision can be individually defensible while the system as a whole becomes fragile in ways no test suite observes.

This is a claim, and it deserves its boundary stated: the argument is about systems that operate under conditions any distributed system can meet — degraded dependencies, cold caches, concurrent load, synchronized expiry. For a bounded system with a stable environment, the inherited defaults may coincide with the correct design, and the distinction costs more than it returns. The frame is a habit with a price; it is worth paying where the operating conditions are real, and it is not always worth paying everywhere.

## The Decisions Are Small and Recurring

The practical objection to architecture-centric thinking is cost: if every decision opens onto the whole system, no implementation would ever finish. The objection fails for a specific reason — the architecture-centric questions are not arbitrary. They follow recognizable patterns, because distributed systems fail in classifiable ways.

Consider what the second version of the endpoint actually decided, and how general each decision is:

- **A bounded cache read.** Redis is a dependency like any other; its failure should not block the request path it serves. The decision generalizes: every infrastructure dependency needs a time budget, and the budget needs a fallback behavior.
- **A defined service timeout with a typed degraded response.** A degraded downstream should produce a signal the caller can act on — retry, fall back, tell the user the data is not trustworthy — not an exception that surfaces wherever it happens to land. The decision generalizes: every dependency boundary needs an explicit degraded-mode contract.
- **Write-behind caching.** A cache write that fails after the source-of-truth read is a performance loss, not a correctness failure; it should not make the caller wait. The decision generalizes: side effects on the response path should be separable from the response itself.
- **A jittered TTL.** When many entries expire together, the cache's own recovery becomes a load spike on the source of truth. The decision generalizes: anything that synchronizes — retries, cache expiries, scheduled jobs — needs deliberate desynchronization.

Timeout boundaries, fallback behavior, cache invalidation, idempotency under retry, backward compatibility of interface changes — these are the recurring concerns of distributed systems. They are not an infinite field. An engineer who has internalized them can apply them systematically to a new situation, and the application cost per decision is a small, repeatable increment. The habit is what carries the cost, not the depth of any single analysis.

This is why the shift is better understood as a change of habit than as a change of seniority. Junior engineers lack the library of failure modes, so the architecture frame generates questions they cannot yet answer, and the implementation frame generates answers they can verify — they default to it for sound reasons. Senior engineers often have internalized the questions so thoroughly that they experience architectural judgment as instinct rather than method, which is precisely why it is hard for them to teach. The middle of the career — enough experience to know the questions matter, not enough to answer them with confidence — is where deliberate practice of the questions has the greatest leverage.

## The Practice

Architectural thinking is not acquired by reading about it. It is acquired by appending a specific set of questions to every implementation decision, before the implementation begins:

- What happens to the callers of this component when it behaves unexpectedly — returns a wrong value, takes ten times as long as expected, or becomes completely unavailable?
- What does the failure of this component look like in the system's observability infrastructure — the distributed trace, the structured log, the metric dashboard?
- What implicit contracts have callers formed with this component's current behavior, and which of them does this change violate?
- What is the worst-case behavior of this implementation at ten times the expected load, and is that worst case acceptable or catastrophic?

These questions do not demand elaborate answers at every decision point. Many implementations are simple enough that the answers are immediately obvious. The habit of asking them — consistently, at the moment of decision — is what converts the finite question set into working judgment. An engineer who asks them is not doing more work; they are distributing a fixed cost across the decisions that matter, instead of paying it all at once in production.

## Why This Matters for AI-Assisted Engineering

This is the precise context in which AI-assisted development tools become more than code generators. A language model trained on the engineering corpus has encoded the patterns of the recurring concerns above. It can surface them on demand, at the moment a developer is making a specific decision, without requiring that developer to have personally encountered every failure mode in production.

But it can only supply that knowledge usefully to an engineer who is already asking the questions. The tool does not supply architectural judgment; it supplies the knowledge base that informs it. To an engineer operating in the architecture-centric frame, the tool is a fast way to enumerate the plausible operating states of a component and the decisions each state requires. To an engineer operating only in the implementation-centric frame, the same tool is a faster way to produce more inherited defaults — more code whose behavior under real conditions was never decided. The value of the tool is proportional to the frame of the engineer using it, and that proportionality is not incidental; it is the difference between infrastructure for judgment and an accelerator of the Knowledge Entropy Loop described in the previous section.

The next section of the book examines this interaction directly: how AI tools function as amplifiers of architectural judgment rather than substitutes for it, and why that distinction has direct consequences for the quality of systems built with AI assistance.

## Boundaries of the Argument

- The argument concerns systems whose operating conditions include degraded dependencies, cold caches, and real concurrent load. It does not claim that every implementation in every system must be architecture-centric; bounded systems with stable environments may legitimately inherit their defaults.
- The distinction between designed decisions and inherited defaults is descriptive, not moral. Inherited defaults are not "bad code"; they are undecided behavior, and undecided behavior is acceptable where the stakes are low and the environment is stable.
- Architecture-centric thinking does not replace implementation skill. Code that makes correct decisions at the system level still has to be correct code at the unit level; the frames are complementary, and the implementation frame remains necessary.
- The finite question set is not a checklist that produces correct systems by itself. It is a habit that produces the right questions at the right moment; the quality of the answers still depends on the engineer's knowledge of their actual system.

## The Section

Section 03 of *AI-Assisted Professional Engineering with .NET* — *The Architectural Shift: From Implementation to Judgment* — develops this argument in full, with the complete dual implementation of the endpoint, the diagram of the two frames, and the detailed mechanics of the shift. It is published as release `v0.1.2`, in English and Arabic, with PDF and DOCX editions.

If you have watched a system fail under conditions no test suite predicted, you have seen the difference between correctness and reliability. The question the next section takes up is what changes when the tool that writes the code also has the knowledge to inform the decisions — and what happens when it does not.

---

## Engineering Series

Previous

[**← 012-The Breakdown of Traditional Workflows at Scale: Why Standard Practices Fail in Distributed Systems**](../../v0.1.1/012-The-Knowledge-Entropy-Loop/article.en.md)

---

## Continue the Journey

This essay is drawn from **Chapter 1, Section 3** of *AI-Assisted Professional Engineering with .NET*. The complete manuscript section contains the full dual implementation, the frame diagram, and the detailed analysis of why the shift is difficult and how it is practiced.

- **GitHub Repository:** <https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET>
- **Release v0.1.2 Asset Bundle:** <https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET/releases/tag/v0.1.2>
- **Full Manuscript Section 03:** <https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET/blob/main/book/chapters/Chapter-01/sections/section-03.en.md>

---

*In Section 03, we established that reliability is the cumulative product of designed decisions rather than inherited defaults, and located the shift in a finite set of recurring questions. In Section 04, we examine how AI tools function as amplifiers of this judgment rather than substitutes for it — and why the distinction is not theoretical but has direct consequences for the quality of systems built with AI assistance.*