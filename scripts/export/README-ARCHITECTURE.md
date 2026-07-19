# ProfBook Publishing Pipeline

## Architecture Specification

Version: 2.0
Status: Draft (Next Generation)

---

# 1. Purpose

This document defines the architecture of the ProfBook publishing system.

Its purpose is to ensure that every component has one clear responsibility and that
new features can be added without modifying unrelated parts of the pipeline.

This document is the architectural contract for every file inside:

scripts/export/

---

# 2. Design Principles

The pipeline SHALL follow these principles.

1. Separation of Concerns
2. Single Responsibility
3. Full File Generation
4. Renderer Isolation
5. Reproducible Builds
6. Engine Independence
7. Production First

---

# 3. High Level Pipeline

                Markdown
                    │
                    ▼
        Markdown Preprocessor
                    │
                    ▼
        Normalized Markdown
                    │
        ┌───────────┴────────────┐
        │                        │
        ▼                        ▼
     DOCX Builder          PDF Builder
        │                        │
        ▼                        ▼
      DOCX               Renderer Dispatcher
                                 │
             ┌───────────────────┼───────────────────┐
             │                   │                   │
             ▼                   ▼                   ▼
        LuaLaTeX            XeLaTeX          wkhtmltopdf

---

# 4. Responsibilities

## Export-Manuscript.ps1

Responsible for:

- Reading parameters
- Loading metadata
- Calling preprocessors
- Selecting renderer
- Executing Pandoc
- Executing PDF engine
- Writing output files

Must NOT contain rendering logic.

Must NOT contain LaTeX code.

Must NOT contain CSS.

Must NOT contain engine-specific layout decisions.

---

## Renderer

Responsible only for rendering.

Every renderer owns:

- command line
- temporary files
- engine options
- cleanup

Nothing else.

---

## Theme

Theme is responsible only for appearance.

Theme must never:

- execute shell commands
- inspect filesystem
- choose renderer
- modify metadata

---

## Markdown Preprocessor

Responsible for:

- Mermaid
- C# snippets
- Include expansion
- Markdown normalization

Nothing else.

---

# 5. Rendering Targets

The publishing system officially supports:

- DOCX
- LuaLaTeX
- XeLaTeX
- wkhtmltopdf

Every target is considered first-class.

No renderer is considered "temporary".

---

# 6. Renderer Independence

Every renderer may have its own implementation.

Feature parity is desired.

Implementation parity is NOT required.

Example:

Code blocks may use:

LuaLaTeX:
    fancyvrb

XeLaTeX:
    fancyvrb

wkhtmltopdf:
    HTML/CSS

This is acceptable.

---

# 7. Full File Regeneration Rule

Files are never patched.

Files are regenerated.

Every generated file replaces the previous version completely.

No incremental edits.

No "insert this line".

No "replace line 52".

---

# 8. KEEP / ROLLBACK Rule

Every experiment receives:

Experiment ID

Result

Decision

Allowed decisions:

KEEP

ROLLBACK

Nothing else.

No experiment remains in an unknown state.

---

# 9. Experiment Rules

One experiment.

One variable.

One PDF.

One decision.

Changing multiple variables simultaneously is forbidden.

---

# 10. File Size Rule

Large responsibilities must be split.

Target size:

200–400 lines per file.

Files exceeding this size should be reviewed for decomposition.

---

# 11. LaTeX Modules

Future layout modules:

layout.tex

typography.tex

codeblocks.tex

tables.tex

figures.tex

covers.tex

colors.tex

headers.tex

toc.tex

pdf-theme.tex

The master file should only orchestrate modules.

---

# 12. Production Quality

The goal is not merely to generate PDFs.

The goal is to produce books suitable for professional publication.

Every implementation decision should be evaluated against this objective.

---

End of document.