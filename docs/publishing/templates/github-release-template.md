# GitHub Release Template

> Permanent template for creating project releases.
>
> Use for every section release and maintenance release (see the
> [Release Strategy](../../engineering/ReleaseStrategy.md) and
> [Release Policy](../release-policy.en.md)).
>
> To create a release: copy this template, update the title, summary, and
> asset list, then publish through `gh release create`.

---

# VERSION — TITLE

<!-- Version | Title
     Examples:
     v0.1.0  | Chapter 1 — Section 1
     v0.1.1  | Chapter 1 — Section 2
     v0.1.1  | Maintenance 1 (v0.1.1-maintenance.1) -->

**Section / Release Title**

<!-- One sentence naming the section or the maintenance scope. -->

This release marks the publication of [Section Y of Chapter X / corrections to vX.Y.Z] of **AI-Assisted Professional Engineering with .NET**.

[Summary paragraph — what this release delivers and why it is a meaningful engineering milestone.]

---

## What's Included

- [Chapter X — Section Y (Markdown)]
- PDF edition
- DOCX edition
- Updated engineering documentation
- Publishing infrastructure improvements

<!-- For maintenance releases, replace content items with corrections:
- Broken links fixed
- Documentation corrections
- Code sample fixes
- PDF/DOCX export fixes
- Technical corrections -->

---

## Engineering Documentation

[Optional — update or remove. Example:]

The repository now includes public engineering documentation describing the project's philosophy, publishing workflow, release strategy, and long-term vision.

---

## Release Assets

- Markdown manuscript
- PDF
- DOCX

---

## Repository

https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET

---

Thank you to everyone following the project.

See you in the next published section.

---

# Release Checklist

Before publishing, verify (see [Release Policy](../release-policy.en.md)):

- [ ] Repository state is stable and pushed to `main`.
- [ ] Section source is committed (`book/chapters/Chapter-XX/sections/section-YY.{ar,en}.md`).
- [ ] Exports regenerated: PDF (EN/AR) + DOCX (EN/AR).
- [ ] README, roadmap, and project status are up to date.
- [ ] Engineering articles (if applicable) are published and the dashboard is updated.

# Publish Command

```text
gh release create vX.Y.Z \
  --target main \
  --title "Chapter X — Section Y: Section Title" \
  --notes-file release-notes.md \
  --latest \
  exports/pdf/sections/Chapter-XX/section-YY.ar.pdf \
  exports/pdf/sections/Chapter-XX/section-YY.en.pdf \
  exports/docx/sections/Chapter-XX/section-YY.ar.docx \
  exports/docx/sections/Chapter-XX/section-YY.en.docx
```
