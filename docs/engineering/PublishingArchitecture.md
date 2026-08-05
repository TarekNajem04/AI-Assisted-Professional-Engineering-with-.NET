# Publishing Architecture

> Strategic Document
>
> This document explains how the project thinks about publishing.
>
> It describes the architectural principles behind the Markdown publishing platform that powers the manuscript. Technical implementation details are documented separately in the [pipeline architecture](../EN/architecture.md).

---

# Guiding Principle

Markdown is the single source of truth.

The manuscript is written in bilingual Markdown files. Every published format — DOCX and PDF — is generated from that source and never edited directly.

Source files are the truth.

Exports are derived artifacts.

---

# Architectural Principles

## 1. Deterministic Builds

The same source and the same configuration always produce the same output.

A successful export is reproducible by anyone who follows the documented workflow.

## 2. Renderer Isolation

Each renderer (PDF, DOCX, cover) owns its complete argument list, its temporary files, its engine options, and its cleanup.

No rendering logic lives in the orchestrator.

This keeps every output format independent and replaceable.

## 3. Full File Regeneration

Output files are regenerated completely. They are never patched.

There is no "insert line 52" in the pipeline.

## 4. Production First

Output must be suitable for professional publication.

The pipeline exists to produce high-quality engineering documents, not prototypes.

## 5. Config-Driven Extension

The pipeline is extended through configuration, not by rewriting scripts:

- New output layout → `SourceLayouts` entry.
- New PDF engine → engine name plus an argument builder.
- New visual style → a style profile.

---

# Publication Model

```text
Markdown Source
     ↓
Export Pipeline
     ↓
DOCX  +  PDF  (+ Covers)
```

The pipeline processes a source file through five stages:

```text
[0] Placeholder cleanup
[1] Context, style profile, DOCX validation, output cleanup
[2] Preprocessors (Markdown → Mermaid → C# → Covers)
[3] PDF generation
[4] DOCX generation
[5] Workspace cleanup
```

---

# Bilingual Architecture

Both languages are first-class citizens.

Language is carried as a file suffix (`section-01.ar.md`, `section-01.en.md`), never as a folder, so each bilingual pair lives side by side in one directory and cannot drift apart.

PDF generation uses LuaLaTeX with `arabic-setup.tex` for right-to-left layout; DOCX generation uses RTL directives and validated styles.

---

# Style Profiles

Visual appearance is data, not hardcoded logic.

Each profile under `scripts/export/styles/<profile>/` provides:

- `profile.json` — configuration.
- `generate-docx-style.py` — DOCX reference template generator.
- `generate-pdf-style.py` — LaTeX theme generator.
- `pdf-theme.tex` — PDF theme template.

Profiles (`oreilly`, `apress`, `default`) are validated before use.

---

# The Platform Is a Product

The publishing platform is intentionally developed as an independent engineering asset.

It powers this manuscript today, but its architecture is designed to support future technical books, documentation projects, and professional publishing workflows.

---

# Relationship to Policies

- The [Release Strategy](./ReleaseStrategy.md) defines how publications are released.
- The [Publishing Strategy](../publishing/publishing-strategy.en.md) defines the publication process.
- This document defines the architecture that implements both.
