# AI-Assisted Professional Engineering with .NET

# Versioning Policy

## Purpose

This document defines the versioning model used by the project.

The objective is to provide a predictable and maintainable release process throughout the lifecycle of the book.

Version numbers should represent meaningful publication milestones rather than routine repository activity.

This document is the **operational implementation** of the [Release Strategy](../engineering/ReleaseStrategy.md). The strategy defines *why* we release the way we do; this document defines *how* version numbers are assigned.

---

# Core Principle

Not every commit is a release.

Not every publication is a release.

Not every release requires a new edition.

The project distinguishes between:

```text
Commit
    ↓
Publication
    ↓
Tag
    ↓
Release
    ↓
Edition
```

Each stage serves a different purpose.

---

# Semantic Versioning Model

The project follows a publication-oriented versioning model built on Semantic Versioning:

```text
0.<chapter>.<section>
```

Where:

* The **minor** digit represents the current chapter.
* The **patch** digit represents the published section within that chapter.

Examples:

```text
v0.1.0
v0.1.1
v0.2.0
v1.0.0
```

---

# Version Categories

## Foundation Releases

Used during repository foundation.

Example:

```text
v0.0.1
```

Purpose:

* Repository initialization
* Publication framework
* Governance documents
* Publishing infrastructure

Foundation releases do not imply book completion.

The foundation release `v0.0.1` is published.

---

## Section Releases

Each published section is released with a version whose **minor** digit identifies its chapter and whose **patch** digit identifies the section within that chapter.

Examples:

```text
v0.1.0   Chapter 1 — Section 1
v0.1.1   Chapter 1 — Section 2
v0.1.2   Chapter 1 — Section 3
v0.2.0   Chapter 2 — Section 1
```

Sections are release units.

Every published section receives a GitHub Release.

---

## Maintenance Releases

To preserve the ability to publish corrections without disturbing the section numbering, maintenance releases are anchored to the version they correct.

Examples:

```text
v0.1.1-maintenance.1
v0.1.1-maintenance.2
```

Maintenance releases are reserved for:

* Broken links
* Documentation corrections
* Code sample fixes
* PDF/DOCX export fixes
* Technical corrections

They must **never** introduce new engineering content.

---

## Major Releases

Example:

```text
v1.0.0
```

Represents:

* Complete public edition
* Stable structure
* Stable publication workflow
* Mature manuscript

---

# Section Publications

A section is the primary publication unit.

Publishing a section follows the release workflow defined in the [Release Strategy](../engineering/ReleaseStrategy.md):

```text
Write Section
    ↓
Review
    ↓
Export Generation
    ↓
Commit
    ↓
Push
    ↓
GitHub Release
    ↓
Publish Announcement
```

---

# Tag Policy

Tags represent publication milestones.

Tags should be created only when:

* A significant milestone is reached.
* Release notes are available.
* Repository state is stable.

Maintenance releases receive their own tags (for example, `v0.1.1-maintenance.1`).

Tags should not be created for routine commits.

---

# Release Policy

Every GitHub Release should include:

* Release title
* Release notes
* Summary of completed work
* Relevant publication links

When available, releases may also include:

* PDF exports
* DOCX exports

---

# Release Assets

Official release artifacts may include:

```text
PDF (EN)
PDF (AR)
DOCX (EN)
DOCX (AR)
```

Artifacts should always be generated from repository content associated with the release tag.

---

# Release Roadmap

## v0.0.1

Repository Foundation — **released**.

Contents:

* Repository structure
* Introduction
* Roadmap
* Publishing policies
* Contribution policies

---

## v0.1.0

Chapter 1 — Section 1: The Complexity Crisis in Modern Software Systems

Contents:

* Section source
* Export artifacts
* Release notes

---

## Future Releases

Future releases follow the publication model:

```text
v0.1.1   Chapter 1 — Section 2
v0.1.2   Chapter 1 — Section 3
...
v0.2.0   Chapter 2 — Section 1
```

Each release should represent meaningful progress in the manuscript.

---

# Release Notes

Every release should include release notes.

Release notes should summarize:

* New content
* Major improvements
* Structural changes
* Publication milestones

The objective is to make project evolution easy to follow.

---

# Publication Integrity

A release should always represent a stable and reproducible state.

Readers should be able to:

* Access repository content
* Generate exports
* Review release history

using only the information available in the tagged version.

---

# Relationship to Other Policies

* [Release Strategy](../engineering/ReleaseStrategy.md) — the high-level architectural policy.
* [Release Policy](../publishing/release-policy.en.md) — the operational release process.

---

# Living Document

This policy may evolve as the project matures.

Any modifications should preserve the project's commitment to predictable and meaningful publication milestones.
