# 📘 **book/samples/README.md — Engineering Edition**
### *C# 14 / .NET 8 Sample Projects for AI‑Assisted Programming*

---

# AI‑Assisted Programming — Sample Projects

This directory contains the full set of **C# 14 / .NET 8** sample projects used throughout the book
**AI‑Assisted Programming: Professional Software Engineering With Modern AI Tools**.

Each sample demonstrates a real engineering concept from its corresponding chapter, including:

- AI‑assisted workflows
- RAG pipelines
- multi‑agent orchestration
- local AI execution (Ollama)
- cloud AI integration
- debugging and refactoring workflows
- architectural patterns and anti‑patterns

**Important:**
Not all projects follow Spec‑Kit.
Only the projects associated with **Spec‑Driven Development (Ch. 11–14)** use Spec‑Kit structure.
All other samples are standalone engineering examples.

---

# 1. Directory Structure

All samples follow the canonical structure:

```
samples/[ChapterNumber]_[ChapterName]/[ProjectName]
```

### Naming Rules

- **ChapterNumber** — two‑digit number (e.g., `01`, `07`, `13`)
- **ChapterName** — PascalCase version of the chapter title
- **ProjectName** — PascalCase project identifier

### Examples

```
samples/03_PromptEngineering/DeterministicPromptingDemo
samples/06_DotNetAIIntegration/OpenAIServiceLayer
samples/08_AdvancedAI/RagPipelineMinimalApi
samples/09_Ollama/LocalInferenceDemo
samples/13_SpecDrivenDevelopment/SpecKitWorkflowExample
```

Each project folder contains:

- its own `.slnx` solution
- a standalone `.csproj`
- source code under `/src`
- optional `/tests` folder
- optional `/docs` folder for diagrams

---

# 2. Technical Standards

All sample projects MUST comply with:

### Runtime & Language

- **.NET 8.0 LTS**
- **C# 14.0**

### Engineering Requirements

- PascalCase folder and project naming
- Clean Code & SOLID principles
- deterministic behavior where applicable
- async/await for all I/O operations
- DI‑first architecture
- minimal API patterns where appropriate
- resilience via Polly (when relevant)
- logging via built‑in .NET logging abstractions

### AI Integration Requirements

Samples MAY include:

- OpenAI / Anthropic / Gemini clients
- Semantic Kernel
- Ollama local inference
- embeddings + vector search
- multi‑agent orchestrators
- AI‑driven middleware
- AI‑generated tests

All AI usage MUST follow the book’s safety and determinism rules.

---

# 3. Spec‑Kit Projects (Chapters 11–14 Only)

Only the following chapters include **Spec‑Kit‑structured projects**:

- **Chapter 11 — Writing Specifications Manually Before Automation**
- **Chapter 12 — Orchestration Collapse & Overengineering**
- **Chapter 13 — Spec‑Driven Development With GitHub Spec Kit**
- **Chapter 14 — AI‑Assisted Engineering Workflows**

These projects follow:

- Spec‑Kit folder structure
- specification‑first workflow
- deterministic planning
- validation pipelines
- structured orchestration

All other chapters use **standalone educational samples**, not Spec‑Kit.

---

# 4. How to Run a Sample

Each project is fully standalone.

### Option A — Visual Studio 2022 (v17.12+)

```
cd samples/[Chapter]/[ProjectName]
```

Open the `.slnx` file and run.

### Option B — VS Code

Open the folder → ensure C# Dev Kit → run.

### Option C — .NET CLI

```
dotnet run --project [ProjectName]
```

---

# 5. Sample Categories

Samples are distributed across chapters according to the book’s conceptual progression:

- **Foundations (Ch. 1–5)**
  Prompting, deterministic workflows, AI collaboration.

- **Applied Engineering (Ch. 6–10)**
  .NET AI integration, debugging, refactoring, RAG, Ollama, cost optimization.

- **Specifications & Workflows (Ch. 11–14)**
  Spec‑Kit projects, orchestration, workflow coordination.

- **Professional AI Engineering (Ch. 15–16)**
  architecture, governance, safety.

---

# 6. Quality Requirements

All samples MUST:

- compile successfully
- run independently
- include meaningful engineering value
- avoid trivial examples
- avoid decorative or filler code
- reflect real production patterns

Samples MUST NOT:

- include placeholder logic
- include speculative APIs
- rely on unstable or experimental libraries

---

# 7. Additional Documentation

Some samples include:

- Mermaid diagrams (`.mmd`)
- architecture notes
- workflow explanations
- validation rules

These appear under:

```
samples/[Chapter]/[Project]/docs/
```

---

# 8. Contribution Rules

Contributions MUST follow:

- PascalCase naming
- .NET 8 / C# 14 standards
- Clean Code & SOLID
- deterministic behavior
- constitution.md governance

Spec‑Kit rules apply **only** to Spec‑Kit chapters (11–14).

All contributions MUST pass the **7‑Gate Validation Pipeline**.

---

# 9. Final Notes

These samples are **production‑grade engineering demonstrations**, not toy examples.
They reinforce the book’s core principles:

- AI amplifies disciplined engineers
- specifications stabilize workflows
- architecture governs AI usage
- hybrid cloud + local AI systems
- deterministic workflows prevent drift
