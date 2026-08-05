# Release Strategy

## Purpose

This document defines the long-term release strategy for the **AI-Assisted Professional Engineering with .NET** project.

Unlike traditional software projects, each release represents a meaningful engineering milestone rather than only a collection of code changes.

Every release should tell a story.

Every release should deliver knowledge.

This document is the **high-level architectural policy** for release and versioning decisions. The operational details of numbering and the operational release process are defined in:

- [Versioning Policy](../versioning/versioning-policy.en.md) — the numbering model and version categories.
- [Release Policy](../publishing/release-policy.en.md) — the operational release process, requirements, and assets.

All three documents must remain consistent and reference each other.

---

# Guiding Principle

This repository is more than a software project.

It combines:

* Technical Writing
* Documentation Engineering
* Publishing Infrastructure
* Reusable Libraries
* AI-Assisted Engineering
* Knowledge Engineering

Therefore, releases are organized around **engineering milestones**, not merely source code modifications.

---

# Versioning Strategy

The project follows Semantic Versioning as a communication mechanism, but the version numbers reflect publishing milestones as well as software evolution.

The publication model maps version numbers to the manuscript directly:

```text
0.<chapter>.<section>
```

* The **minor** digit represents the current chapter.
* The **patch** digit represents the published section within that chapter.

---

## Foundation Releases

```
v0.0.x
```

Purpose:

Establish the engineering foundation.

Typical contents:

* Repository structure
* Branding
* Documentation
* Publishing platform
* Automation
* Initial engineering articles

Example:

```
v0.0.1
Repository Foundation
```

---

## Section Releases

The first digit after `0.` represents the current chapter.

The patch version represents the published section within that chapter.

Example:

```
v0.1.0
Chapter 1
Section 1

v0.1.1
Chapter 1
Section 2

v0.1.2
Chapter 1
Section 3

...

v0.2.0
Chapter 2
Section 1
```

This keeps release numbers meaningful while allowing readers to understand the publication timeline immediately.

---

## Maintenance Releases

To preserve the ability to publish corrections without disturbing the section numbering, the project introduces maintenance releases.

Example:

```
v0.1.1-maintenance.1
v0.1.1-maintenance.2
```

A maintenance release is anchored to the version of the content it corrects.

Maintenance releases are reserved for:

* Broken links
* Documentation corrections
* Code sample fixes
* PDF/DOCX export fixes
* Technical corrections

They must **never** introduce new engineering content.

---

# Release Contents

Every release should contain, whenever applicable:

* Markdown source
* DOCX export
* PDF export
* Updated documentation
* Engineering articles
* Release notes

---

# Release Naming

Every release should have both:

Version Number

Example

```
v0.1.0
```

Human-readable Title

Example

```
Chapter 1 — Section 1
The Complexity Crisis in Modern Software Systems
```

The title should describe the engineering milestone rather than only the technical changes.

---

# Engineering Articles

Engineering articles are part of the release.

They document:

* Design decisions
* Architecture
* Lessons learned
* Publishing workflow
* AI-assisted engineering process

They are considered first-class release assets.

---

# Release Workflow

For every published section:

1. Complete the Markdown manuscript.
2. Generate DOCX.
3. Generate PDF.
4. Review exported documents.
5. Publish to GitHub.
6. Create a GitHub Release.
7. Publish the corresponding engineering article(s).
8. Update the article dashboard.
9. Announce the release through publication channels.

---

# Long-Term Vision

The repository grows incrementally.

Readers should be able to follow the project section by section, release by release.

Every release improves not only the manuscript but also the engineering platform itself.

Eventually:

```
v1.0.0
```

will represent the first complete edition of the book together with a mature publishing ecosystem.

The release history itself should tell the complete engineering story.

---

This strategy reflects the current publication workflow and may evolve as the project expands to include additional engineering assets beyond the manuscript.
