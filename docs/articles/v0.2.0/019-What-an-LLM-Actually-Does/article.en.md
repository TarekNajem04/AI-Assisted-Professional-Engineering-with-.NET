# What an LLM Actually Does: The Generative Loop Behind Every Token

[Medium](https://tareknajem04.medium.com/ai-assisted-professional-engineering-with-net-0a66c74d0592)
[LinkedIn](https://www.linkedin.com/pulse/ai-assisted-professional-engineering-net-tarek-najem-hispe)

*Most engineering judgments about AI rest on a model of the machine, and most of those models grant the machine faculties it does not have. This essay describes the actual mechanism — because every decision about where generated output can be trusted is downstream of it.*

## The Model in Most Heads Is Wrong

Ask an engineer how a language model answers a question and the description usually includes three things: a memory of the conversation, a database of facts it consults, and something like a plan behind the response. None of these exist in the architecture. What exists is a single repeated operation: given numbers representing the text so far, produce a probability distribution over the next number, sample one, append it, and repeat. Everything else — the fluency, the apparent reasoning, the code that compiles — is what this operation produces when the numbers represent language.

## The Loop, Stated Plainly

Text becomes token IDs through a fixed-vocabulary lookup, token IDs become vectors through a learned embedding table, and the transformer layers turn those vectors into a probability distribution over the vocabulary. One token is sampled and appended. Then the entire forward pass runs again over the prompt plus everything generated so far. A 500-token answer is 500 full passes, each conditioned on all previous tokens — including the mistaken ones. There is no separate planning step anywhere in this process, and no point at which the model stops to evaluate what it has produced.

## The Cost Model Is Not What It Looks Like

Tokens are neither words nor characters, and the mapping is uneven: common prose averages roughly four characters per token, while C# dense with project-specific identifiers or GUIDs costs substantially more per character. Worse, the context window is a fixed-size buffer shared by everything — system prompt, history, retrieved documents, and output so far — re-processed on every pass, so latency grows non-linearly with context length. A .NET service that concatenates "everything possibly relevant" into the prompt is making the same category of decision as one that allocates an unbounded buffer per request: it works until inputs grow, then fails outright or degrades in ways that are hard to diagnose.

## Temperature Is a Configuration Decision

Sampling is controlled by temperature: at zero the likeliest token is always taken; above zero, lower-probability tokens have a real chance, producing varied but internally "equally valid" output. The same prompt run twice can therefore yield different sequences — a genuine source of variation with no equivalent in a deterministic method call. Exact-string test assertions do not transfer to generated output without an explicit architectural decision: how much variation is acceptable, and how will it be detected.

## The Absences Are the Point

No persistent memory exists between requests beyond re-sent context. No fact database is consulted — everything the model "knows" is frozen into weights at training time. No plan exists independently of the token sequence. And the confident tone of generated prose is not a confidence signal; it is the most likely continuation of confident-sounding text. The mechanism emitting a real API name and the one emitting a plausible-but-nonexistent name are identical: proximity in embedding space, not a lookup. The model has no way of knowing whether it is right; it has only a distribution over what comes next.

## Caveats

- This describes current transformer-based models. Sizes and context windows will change; the loop persists.
- "No memory" concerns a single model call. Products legitimately add memory around it — retrieval, stores, history management — but that is architecture outside the model and must be evaluated as such, not credited to it.
- Deterministic settings narrow variation; they do not change the architecture. A temperature-zero deployment is still sampling from a distribution — it has only agreed always to take the top.

## Why This Comes First

Three practices change once the loop is visible. First, risk assessment moves before integration: is this task dense in training data or a rare interaction of features? The answer sets the verification level. Second, prompt structure becomes deliberate: the most important information goes first and last, and context is budgeted instead of accumulated. Third, instruction placement matters: early tokens shape every later distribution, so late corrections fight entrenched statistical patterns. The rest of this chapter is these consequences worked out in detail.

## The Section, and What Comes Next

The first section of Chapter 2 of **AI-Assisted Professional Engineering with .NET** — *What an LLM Actually Does: Tokens, Embeddings, Context, and Inference* — develops this mechanism in full, with the complete generative diagram, the token-economics analysis, and a minimal .NET streaming sample that makes per-token generation visible. It is published as release `v0.2.0`, in English and Arabic, with PDF and DOCX editions.

The project publishes section by section so each claim can be tested against experience before the next builds on it. The next section takes up the complementary question: if the machine only predicts the next token, where does the persuasive impression of comprehension and reasoning come from?

If this mechanism matches what you have observed in production — or contradicts it — I would like to hear where. That is the point of publishing it as a section rather than as a finished book.

*If fluency is the product of the loop rather than evidence of understanding, what should change in how we review generated output?*

---

## Engineering Series

Previous

[**← 018-Assumptions-Made-Explicit: A Payment-Service Case Study in Architectural Judgment**](../../v0.1.6/018-Assumptions-Made-Explicit/article.en.md)

---

## Continue the Journey

This essay is drawn from **Chapter 2, Section 1** of *AI-Assisted Professional Engineering with .NET*. The complete manuscript section contains the full generative-loop diagram, the token-economics analysis, the context-window discussion, and the minimal .NET streaming sample with accompanying code.

- **GitHub Repository:** <https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET>
- **Release v0.2.0 Asset Bundle:** <https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET/releases/tag/v0.2.0>
- **Full Manuscript Section 01:** <https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET/blob/main/book/chapters/Chapter-02/sections/section-01.en.md>

---

*In Section 01, we traced the generative loop from token to distribution to sample, and saw how it governs token economics, context budgeting, and sampling variation — and why the machine has nothing resembling memory, lookup, or planning, which is why apparent confidence is never a confidence signal. Section 02 examines the other face: where the persuasive impression of comprehension comes from, and where it breaks down.*
