# AI-Assisted Professional Engineering with .NET

# Release Policy

## Purpose

This document defines the official release process for the project.

The objective is to ensure that every release represents a stable, reproducible, and meaningful publication milestone.

Releases are intended to communicate progress to readers and provide durable reference points throughout the life of the manuscript.

---

# Release Principles

Every release should be:

* Reproducible
* Reviewable
* Traceable
* Stable
* Documented

A release is not merely a Git tag.

A release is a publication event.

---

# Release Types

## Bootstrap Release

Example:

```text
v0.0.1
```

Purpose:

* Establish repository foundation
* Publish governance documents
* Publish publication policies
* Publish project roadmap

---

## Chapter Release

Examples:

```text
v0.1.0
v0.2.0
v0.3.0
```

Purpose:

* Publish mature chapter content
* Publish chapter exports
* Record publication milestone

---

## Maintenance Release

Examples:

```text
v0.1.1
v0.1.2
```

Purpose:

* Correct errors
* Improve exports
* Improve documentation

---

## Major Release

Example:

```text
v1.0.0
```

Purpose:

* Publish complete public edition
* Establish stable baseline
* Mark major project milestone

---

# Release Requirements

Before creating a release:

* Repository must be in a stable state.
* Relevant documentation must be updated.
* Table of contents must be synchronized.
* Internal links must be validated.
* Export generation must complete successfully.

No release should be created from an unverified repository state.

---

# Release Checklist

## Documentation

Verify:

* README
* Roadmap
* Project Status
* Publishing Policies

are up to date.

---

## Manuscript

Verify:

* Published content is complete.
* Cross references are valid.
* Navigation links are valid.
* Section ordering is correct.

---

## Exports

Verify:

* PDF (EN)
* PDF (AR)
* DOCX (EN)
* DOCX (AR)

if applicable for the release.

---

## Repository

Verify:

* Clean repository state
* No temporary files
* No local-only artifacts
* No broken references

---

# Release Notes

Every release must contain release notes.

Release notes should include:

* Version number
* Release title
* Summary
* Major additions
* Significant changes
* Future direction

---

# Release Assets

Release assets may include:

```text
PDF (EN)
PDF (AR)
DOCX (EN)
DOCX (AR)
```

Assets should always correspond to the exact tagged version.

---

# Publication Workflow

```text
Content Complete
        ↓
Technical Review
        ↓
Repository Review
        ↓
Export Generation
        ↓
Tag Creation
        ↓
GitHub Release
        ↓
Public Announcement
```

---

# Post-Release Activities

After publication:

* Update project status
* Update roadmap if needed
* Publish announcement
* Collect community feedback
* Record observations

Feedback gathered after release may influence future planning.

---

# Release Ownership

Official releases are created by project maintainers.

Community members may provide feedback, but release authority remains with maintainers.

---

# Living Document

This policy may evolve as publication workflows mature and new requirements emerge.
