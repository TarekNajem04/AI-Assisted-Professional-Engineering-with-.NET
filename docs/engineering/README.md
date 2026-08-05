# Engineering Documentation

> Engineering documentation answers a single question: **how does this project think?**

These documents are not usage manuals. They do not explain how to run a command or configure a tool. They define the identity, vision, philosophy, and long-term strategy of the project — the principles that guide every architectural decision, every publication, and every release.

| Document | Answers |
|----------|---------|
| [Repository Identity](./RepositoryIdentity.md) | Who are we? |
| [Project Vision 2030](./ProjectVision2030.md) | Where are we going? |
| [Engineering Manifesto](./EngineeringManifesto.md) | What do we believe? |
| [Release Strategy](./ReleaseStrategy.md) | How do we release? |
| [Publishing Architecture](./PublishingArchitecture.md) | How do we publish? |
| [Branding Strategy](./BrandStrategy.md) | How do we present ourselves? |

---

# Document Lifecycle

Every engineering document has a lifecycle, just like code:

```text
.tmp/
   ↓
Review
   ↓
Adopted
   ↓
docs/
```

## Stage 1 — `.tmp/` (Workspace)

Unfinished material lives in the gitignored `.tmp/` folder. This includes:

- Drafts of articles and documents
- Incomplete ideas
- Plans for upcoming releases
- Temporary notes
- Experiments

## Stage 2 — Review

A document is promoted only after it is reviewed for:

- Terminology consistency
- Structural consistency
- Cross-reference validity
- Agreement with the current policies

## Stage 3 — Adopted

The document is approved as official project policy or positioning.

## Stage 4 — `docs/`

The adopted document is published under `docs/` and becomes part of the public engineering documentation.

Any document that represents actual project policy must leave `.tmp/` and be published here.

---

# Relationship to Other Documentation

- `docs/EN/` and `docs/AR/` contain **operational usage documentation** (how to use the pipeline).
- `docs/publishing/`, `docs/versioning/`, `docs/governance/`, `docs/roadmap/`, `docs/project-status/` contain the **operational policies** that implement this strategy.
- `docs/engineering/` contains the **architectural policy** — the reasoning behind those operational documents.

---

# Living Documentation

Engineering documentation evolves with the project.

Whenever a policy, identity element, or strategic decision changes, the affected document must be updated in the same change.
