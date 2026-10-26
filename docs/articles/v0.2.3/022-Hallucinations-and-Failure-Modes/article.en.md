# Hallucinations and Failure Modes: Structural, Not Incidental

[Medium](https://tareknajem04.medium.com/ai-assisted-professional-engineering-with-net-22-d291692a2c6e)
[LinkedIn](https://www.linkedin.com/pulse/ai-assisted-professional-engineering-net-tarek-najem-dksge)

*The fabricated API does not announce itself. It arrives fluent, plausible, and confident — because it is produced by the same mechanism that produces correct answers. This essay is about treating that as an engineering constraint rather than a bug to wait out.*

## The Failure That Must Happen

Section 03 ended with a silent-failure mechanism: statistical reasoning produces the most probable next token regardless of whether a reliable pattern exists. Section 04 follows that mechanism to its most notorious outcome — hallucination.

The term borrows from psychology and carries an implication of "making things up," as though a model deliberates and chooses invention. There is no deliberation in it. Given a context for which no reliable pattern exists — or where patterns exist but point in inconsistent directions — the most probable next token is still *some* token, sampled from whatever distributional evidence accumulated during training. The model has no state that distinguishes "I am about to name a real API" from "I am about to emit a token sequence that occupies the syntactic position of an API name." Hallucination is structural rather than incidental: it is what next-token prediction without a truth oracle looks like.

That reframing has immediate consequences for practice. It changes the response from "prompt better to avoid it" — which reduces frequency in some cases but cannot remove the mechanism — to "design systems that assume it will occur and verify before acting."

## A Taxonomy for .NET Work

Failure modes in .NET assistance are not one phenomenon. Four categories matter:

**Fabricated packages and namespaces.** A plausible package name that does not exist on NuGet, or a namespace carried over from an older framework generation. Restore fails loudly — the cheapest category to catch, and often mistaken for the whole problem.

**Invented API overloads.** A method with realistic naming, valid parameter types, and a signature that belongs to a sibling API. This category is more dangerous because it is plausible enough that an engineer unfamiliar with the specific library may accept it under time pressure.

**Outdated pattern signatures.** Suggestions that were correct for an earlier version: synchronous connection handling where the modern idiom is async, `WebClient` where `HttpClient` is expected, patterns whose names bring their older semantics along.

**Plausible-but-wrong architecture guidance.** Advice that sounds like engineering judgment — caching strategy, retry policy, consistency approach — assembled from adjacent patterns rather than from the specific system's constraints. This is the most expensive category, because verification requires the independent judgment the advice was supposed to support.

The categories compound. One fabrication hides inside another: a fabricated package with an invented overload, explained by outdated guidance, reads as a coherent recommendation. And each category demands a different verification response — which is why a single rule ("be careful") fails as a strategy.

## Why It Reads Like Documentation

The fabricated API is convincing for the same reason correct output is convincing: it is drawn from the statistical shape of technical prose. Namespace conventions, method naming patterns, parameter ordering, the explanatory sentences around a code block — the mechanism reproduces all of it with equal fluency whether or not the referent exists.

This is why reading more carefully does not fix the problem. Careful reading tests internal coherence, and the output is internally coherent by construction. What it lacks is correspondence to the external world — a property the text cannot attest to, because the text was produced without consulting it.

## The Confidence Signal Problem

The natural countermeasure — ask the model to hedge when uncertain — does not supply the missing signal. The model has no uncertainty state to report. When it produces hedged language, that language is itself tokens statistically correlated with topics that writers tend to hedge about. The correlation is real but weak, and it does not track the cases where distrust is warranted.

Given that, the engineering response cannot depend on anything the model reports about itself. Verification has to come from outside: from the compiler, from the type system, from tests that exercise failure semantics, from knowledge held by the engineer.

## Designing Against It

Three principles organize the response for .NET teams, plus a fourth that governs maintenance.

**Push detection to the earliest boundary available.** The type system is the cheapest hallucination detector in existence: fabricated APIs on real types do not compile. Strong typing, explicit interfaces, and compile-time contracts convert a semantic risk into a build error. Code that arrives as text into a strongly typed codebase is already being filtered by something more reliable than vigilance.

**Validate at the boundary where AI output enters the system.** Whether the output is a suggestion pasted into code, a generated configuration, or a runtime call, the point of entry should have a validation layer appropriate to its risk: schema checks, known-bad pattern filtering, contract tests.

**Test semantics, not just the happy path.** Generated code tends to be correct at the level of individual statements and wrong in a specific edge case. Test coverage that exercises cancellation, contention, degradation, and failure sequences catches what compilation cannot.

**Expect the risk profile to move.** Libraries released after a model's training cutoff are systematically under-represented; long-documented ones are reinforced. The sparse regions shift with every generation. Verification depth per technology area should be reassessed periodically rather than fixed once at adoption.

## What Tooling Catches, and What It Cannot

Automated checks — compilation, analyzers, dependency validation, package restore — catch the fabricated-symbol categories cheaply and should be assumed as a baseline. They do not catch outdated semantics or wrong architecture guidance; those require tests written against the real constraints and reviewers who know the system.

Which reframes code review culture. Review that reads for familiar shapes confirms exactly what generated code preserves — the shape. Review that constructs failure scenarios ("what happens on the shutdown path? what happens when this dependency degrades?") tests what it does not. The distinction from Section 03 applies here in its most concrete form.

## Caveats

- Frequency varies enormously by model, by technology area, and by how specific the prompt is. The structural argument concerns the mechanism, not a constant error rate.
- Better prompting and retrieval genuinely reduce hallucinations. They do not change the architecture that makes verification necessary.
- The taxonomy is a working instrument, not a closed list; new model capabilities will produce new failure categories alongside the old ones.

## The Section, and What Comes Next

The fourth section of Chapter 2 of **AI-Assisted Professional Engineering with .NET** — *Hallucinations and Failure Modes* — develops this argument in full: the structural diagnosis, the failure taxonomy with concrete .NET examples, the confidence problem, the design principles including the detection-layer code sample, and the discussion of tooling and team practice. It is published as release `v0.2.3`, in English and Arabic, with PDF and DOCX editions.

The next section takes the architectural question directly: if a component inside a deterministic .NET system operates probabilistically, where exactly must its boundary be drawn — and what is allowed to cross it?

*If verification is the only reliable signal, which layer of your system currently owns it — the compiler, the test suite, the reviewer, or nothing at all?*

---

## Engineering Series

Previous

[**← 021-The Limits of Statistical Reasoning: Correct Shape, Wrong Semantics**](../../v0.2.2/021-The-Limits-of-Statistical-Reasoning/article.en.md)

---

## Continue the Journey

This essay is drawn from **Chapter 2, Section 4** of *AI-Assisted Professional Engineering with .NET*. The complete manuscript section contains the failure-mode taxonomy for .NET engineering, the fabrication example with its detection layer, the design principles, and the analysis of hallucination's interaction with code review culture and development-environment infrastructure decisions.

- **GitHub Repository:** <https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET>
- **Release v0.2.3 Asset Bundle:** <https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET/releases/tag/v0.2.3>
- **Full Manuscript Section 04:** <https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET/blob/main/book/chapters/Chapter-02/sections/section-04.en.md>

---

*In Section 04, we treated hallucination as an engineering constraint: structural rather than incidental, varied in its failure categories, invisible in its confidence signal, and answerable only through boundaries that live outside the model — the type system, validation layers, semantic tests, and reviewers who know the system. Section 05 takes up the architectural question: where the probabilistic boundary belongs inside a deterministic .NET system.*
