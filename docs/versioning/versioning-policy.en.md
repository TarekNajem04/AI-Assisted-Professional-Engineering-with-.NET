# AI-Assisted Professional Engineering with .NET

# Versioning Policy

## Purpose

This document defines the versioning and release strategy used by the project.

The objective is to provide a predictable and maintainable release process throughout the lifecycle of the book.

Version numbers should represent meaningful publication milestones rather than routine repository activity.

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

The project follows a simplified semantic versioning model:

```text
MAJOR.MINOR.PATCH
```

Examples:

```text
v0.0.1
v0.1.0
v0.2.0
v1.0.0
```

---

# Version Categories

## Bootstrap Releases

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

Bootstrap releases do not imply book completion.

---

## Chapter Releases

Used when a chapter reaches publication maturity.

Examples:

```text
v0.1.0
v0.2.0
v0.3.0
```

Each chapter release should represent a meaningful increase in available content.

---

## Maintenance Releases

Examples:

```text
v0.1.1
v0.1.2
```

Used for:

* Corrections
* Typographical fixes
* Export improvements
* Documentation updates

No major content expansion should occur within maintenance releases.

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

Sections are publication units.

Sections are not release units.

Publishing a section:

* Does not require a Git tag.
* Does not require a GitHub Release.
* Does not require export artifacts.

Typical workflow:

```text
Write Section
    ↓
Review
    ↓
Commit
    ↓
Push
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

# Initial Release Roadmap

## v0.0.1

Repository Bootstrap

Contents:

* Repository structure
* Introduction
* Roadmap
* Publishing policies
* Contribution policies

---

## v0.1.0

First Published Chapter

Contents:

* Chapter release
* Export artifacts
* Release notes

---

## Future Releases

Future releases will follow the same pattern:

```text
v0.2.0
v0.3.0
v0.4.0
...
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

# Living Document

This policy may evolve as the project matures.

Any modifications should preserve the project's commitment to predictable and meaningful publication milestones.
