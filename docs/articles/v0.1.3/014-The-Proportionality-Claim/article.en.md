# The Proportionality Claim: Why AI Amplifies Judgment, Not Replaces It

[Medium](https://tareknajem04.medium.com/ai-assisted-professional-engineering-with-net-14)
[LinkedIn](https://www.linkedin.com/pulse/ai-assisted-professional-engineering-net-tarek-najem-014)

*The value an engineer extracts from AI tools is proportional to the architectural judgment they bring to those tools — and understanding why changes everything about how you work.*

## The Claim That Runs Against the Narrative

There is a claim about AI-assisted development that is not made often enough, because it runs against the dominant narrative of AI as a universal productivity multiplier: the value an engineer extracts from AI tools is proportional to the architectural judgment they bring to those tools.

This is not a moral argument about whether engineers should develop their skills. It is an engineering observation about how these tools actually function. A language model does not evaluate the quality of the context it receives. It generates a response that is statistically consistent with the patterns in its training data, conditioned on the prompt. If the prompt encodes shallow understanding of the problem — if the engineer asking the question has not yet made the architectural shift described in the previous section — the response will be well-formed code that addresses the stated problem, with no awareness of the unstated architectural context that makes the stated problem the wrong thing to optimize.

The consequence is precise: AI tools applied without architectural judgment produce locally correct code that accumulates systemic risk, at a rate that scales with the velocity the AI provides. An engineer using AI to generate code faster without the judgment to evaluate that code systemically is accelerating the accumulation of knowledge entropy, not reducing it.

The inverse is equally important: AI tools applied with strong architectural judgment become genuinely powerful. An engineer who knows what questions to ask, who can evaluate the systemic consequences of a generated implementation, and who understands which aspects of a problem require human judgment and which can be safely delegated — that engineer finds that AI tools expand their effective reach in ways that would not be possible otherwise.

## Two Engineers, One AI Output

Consider a concrete scenario. An AI generates a Polly resilience pipeline for an external API client — retry policy, circuit breaker, timeout. The implementation is technically correct against general patterns. It compiles, it follows idiomatic .NET conventions, and a surface-level code review would approve it.

Two engineers receive this identical output.

**Engineer A** deploys it unchanged. The configuration is technically correct. Three issues become visible only in production:

The circuit breaker's `MinimumThroughput` of 10 means it requires 10 requests before it can trip. Under low-traffic conditions, the circuit never trips regardless of the failure rate. The downstream service can fail completely without triggering circuit isolation.

The 10-second timeout is longer than the calling endpoint's own timeout of 8 seconds. When the external API hangs, the circuit breaker timeout fires after the caller has already disconnected, creating orphaned downstream connections that consume thread pool slots.

The retry policy handles `500` (InternalServerError) but not `429` (TooManyRequests). When the external API rate-limits the service, the retry policy immediately retries, making the rate-limiting condition worse instead of backing off.

**Engineer B** uses the AI's output as a starting point and asks four questions before deploying:

*What is the actual traffic pattern to this service?* Approximately 2 requests per minute.

*What is this endpoint's own timeout?* 8 seconds.

*Does this API ever return 429?* Yes, at over 100 requests per hour.

*What is the acceptable blast radius if this dependency fails?* Callers get 503.

Engineer B adjusts: retry attempts reduced to 2, 429 added to handled status codes, circuit breaker sampling window extended to 60 seconds for low traffic, minimum throughput lowered to 3, timeout reduced to 5 seconds — below the caller's budget.

The generated code and the deployed code are similar. The differences are small. The production behavior is substantially different. The difference was not in the AI's output — it was in the questions the engineer asked.

## The Four Collaboration Models

The relationship between an engineer and an AI tool is not uniform. It varies depending on the type of task and the type of knowledge required to perform it well. Four collaboration models cover the range of productive interaction.

**Generator mode** is the model most commonly discussed — the AI produces an initial implementation from a description. It is the mode with the highest risk of misuse, because it creates the most complete-looking output and therefore the strongest temptation to treat the output as finished. Generator mode is productive when the engineer provides rich context: the architectural constraints of the system, the failure modes that must be handled, the contracts that must be honored, the performance characteristics required. It is dangerous when the engineer provides only the happy-path requirement and accepts the output without evaluating its systemic implications.

**Reviewer mode** inverts the direction. The engineer writes the implementation; the AI examines it. This mode tends to produce more reliable results than generator mode, because the engineer has invested the implementation energy and is more likely to critically evaluate the AI's feedback. The AI's value here is primarily in surfacing patterns and edge cases the engineer has not considered, suggesting alternatives the engineer can evaluate, and identifying potential issues that benefit from an external perspective. The engineer retains the decision authority entirely.

**Advisor mode** is the most powerful and the least discussed. The engineer presents an architectural problem: a design decision with multiple viable options, a failure mode they are trying to reason about, a constraint they are trying to satisfy. The AI surfaces relevant patterns, historical approaches, and tradeoff considerations from its training. The engineer applies this knowledge to their specific system. The value here is in the AI's ability to surface relevant knowledge quickly — knowledge that would otherwise require research, experienced colleagues, or trial and error to access.

**Executor mode** applies to tasks that are well-specified, routine, and where the engineer can verify correctness easily: generating boilerplate, writing documentation from code, converting between data formats, applying mechanical refactoring patterns. The engineer specifies exactly what is needed; the AI performs the transformation; the engineer verifies the output. This mode carries low risk precisely because the verification step is straightforward.

The common error in AI-assisted development is applying executor-mode expectations to generator-mode tasks: treating a generated implementation as a completed transformation rather than as an initial draft that requires architectural evaluation.

## What AI Provides and What Engineers Supply

The most productive mental model for working with AI tools is the following: the AI provides knowledge; the engineer provides judgment.

Knowledge, in this context, means the patterns, implementations, and engineering approaches that exist in the training corpus. A correctly formulated prompt reliably surfaces relevant knowledge from that corpus — the right resilience pattern for a given failure mode, the idiomatic way to implement a specific .NET API, the common edge cases in a distributed transaction pattern, the typical security considerations for a given class of system.

Judgment means the application of that knowledge to a specific system, with full awareness of that system's specific constraints, history, failure modes, operational characteristics, and organizational context. Judgment is what the engineer supplies that the AI structurally cannot — because it requires exactly the distributed, context-specific knowledge that no training corpus can encode for your particular system.

The collaboration works when these two contributions are correctly separated: the engineer asks questions that retrieve relevant knowledge from the AI, then applies judgment to determine what that knowledge means for their specific system. It fails when the engineer treats the AI's output as judgment-complete — as an answer rather than as evidence to be evaluated.

An AI response is delivered as a confident, complete, well-structured answer — formatted, referenced, self-assured. There is nothing in its presentation that marks it as provisional. The engineer who receives a confident answer must actively reclassify it as evidence: a candidate hypothesis about the correct approach, generated from general patterns rather than from observation of the specific system. The reclassification is a cognitive act, and it is the entire substance of the judgment contribution. Skipping it — treating the response's confidence as a property of the content rather than a property of the language model's generation — is the single most common mechanism by which AI assistance produces systemically incorrect deployments.

## The Generation-Speed Trap

There is a specific and common failure pattern in AI-assisted development that deserves explicit naming: the generation-speed trap. It occurs when the success criterion for a development session becomes the volume of generated code that passes tests, rather than the number of architectural decisions that were understood and correctly evaluated.

In this pattern, generator mode is applied to tasks that actually require advisor mode or reviewer mode. The engineer describes what they want built, receives a complete implementation in seconds, confirms the tests pass, and adds the code to the pull request. The cycle takes ten minutes instead of an hour. Productivity appears to have doubled when measured by completed code.

What this measurement does not capture is the accumulation of unevaluated assumptions. Every implementation accepted without architectural evaluation adds a new layer of implicit decisions to the codebase — decisions that nobody knows about because nobody made them consciously. Over time, the system accumulates archaeological complexity at a much faster rate.

The answer to the generation-speed trap is not a return to writing all code manually. The answer is measuring productivity with a more complete metric: not the volume of generated code that passes tests, but the number of architectural decisions that were understood, formulated, and correctly evaluated. This metric rewards using AI as an amplifier of architectural judgment rather than as a tool for bypassing it.

## How Collaboration Changes as Tools Improve

Some assume that more capable models will shrink the need for human architectural judgment, approaching a point where AI-assisted development becomes a form of autonomous development. This conclusion is wrong, and it grows out of a misreading of the nature of the fundamental constraint.

The constraint is not in the quality of the output the model generates — though quality improves noticeably with every model generation. The constraint is in the context-specific knowledge that real production systems demand. The system you are building carries a history of decisions made in response to specific production incidents, constraints imposed by recent requirements, and implicit contracts formed by how actual consumers have used the service over time. This information does not exist in any training corpus and never will, because it is specific to your system in its specific context.

What changes with more capable models is the surface of the collaboration: the first three collaboration modes expand to cover tasks that are more complex and more technically demanding. A more capable model can generate more sophisticated implementations, review code more precisely, and advise on deeper architectural problems. But the engineer receiving those improved outputs needs deeper architectural judgment to evaluate them correctly, because higher architectural complexity means a wider space of implicit assumptions that may not hold for their specific system.

This dynamic means that investing in architectural judgment compounds its return as tools improve — it does not diminish. The engineer who builds genuine depth now in reasoning about distributed systems, their failure modes, and their operational characteristics is positioning themselves to extract maximum value from every future generation of AI tools.

## Boundaries of the Argument

- The proportionality claim is an engineering observation, not a moral prescription. It describes how AI tools actually function, not how engineers should feel about using them.
- The argument does not claim that AI tools are dangerous. It claims that their danger profile is conditional on the judgment of the engineer using them — and that this conditionality is permanent, not a temporary limitation.
- The four collaboration models are descriptive, not prescriptive. Real engineering work often blends modes within a single session. The value of the taxonomy is in making mode selection conscious rather than automatic.
- The generation-speed trap is not a claim that fast code is bad code. It is a claim that speed without evaluation is risk accumulation — and that the evaluation is the engineer's irreplaceable contribution.

## The Section

Section 04 of *AI-Assisted Professional Engineering with .NET* — *AI as Professional Amplifier: Defining the Relationship* — develops this argument in full, with the complete Polly pipeline example, the four collaboration models in detail, the knowledge-judgment separation, and the analysis of how the relationship evolves as tools improve. It is published as release `v0.1.3`, in English and Arabic, with PDF and DOCX editions.

If you have ever deployed AI-generated code that passed every test and still failed in production, you have seen the proportionality claim in action. The question the next section takes up is where the responsibility boundary lies — the precise line between what an engineer delegates to AI and what they own regardless of who or what generated the code.

---

## Engineering Series

Previous

[**← 013-Reliability Is Designed, Not Inherited**](../../v0.1.2/013-Reliability-Is-Designed-Not-Inherited/article.en.md)

---

## Continue the Journey

This essay is drawn from **Chapter 1, Section 4** of *AI-Assisted Professional Engineering with .NET*. The complete manuscript section contains the full Polly pipeline example, the collaboration model diagram, the knowledge-judgment framework, and the detailed analysis of the generation-speed trap.

- **GitHub Repository:** <https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET>
- **Release v0.1.3 Asset Bundle:** <https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET/releases/tag/v0.1.3>
- **Full Manuscript Section 04:** <https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET/blob/main/book/chapters/Chapter-01/sections/section-04.en.md>

---

*In Section 04, we established that the value of AI tools is proportional to the architectural judgment applied to them, defined the four collaboration models, and identified the generation-speed trap as the primary failure pattern in AI-assisted development. In Section 05, we examine the responsibility boundary — the precise line between what an engineer delegates to AI and what they own regardless of who or what generated the code.*
