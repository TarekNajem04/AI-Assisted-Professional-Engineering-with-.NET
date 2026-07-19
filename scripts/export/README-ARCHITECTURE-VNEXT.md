# Export vNext Architecture

## Why are there files that are not used?

This repository is currently in the middle of an architectural migration.

The original export pipeline has been replaced by a new architecture based on:

- `ExportContext`
- `core/`
- `project/`
- `preprocessors/`
- `pandoc/`

These components form the **active export pipeline**.

---

# Why keep the old files?

The older implementation has intentionally been preserved.

They are **not loaded** by the current pipeline.

They remain in the repository for several reasons:

- historical reference
- architecture comparison
- debugging previous releases
- recovering implementation details
- documenting design evolution

Deleting them during the migration would permanently remove valuable engineering knowledge.

---

# Current Status

## Active

```
scripts/export/core/
scripts/export/project/
scripts/export/preprocessors/
scripts/export/pandoc/
```

These directories contain the implementation used by the export pipeline.

---

## Deprecated

Examples include:

```
scripts/export/exporters/
scripts/export/pdf/
scripts/export/shared/
```

and several legacy helper scripts.

These files are intentionally preserved.

They are **not** part of the active export process.

---

# Repository Policy

During the Export vNext migration:

- Existing code may be marked as deprecated.
- Existing code may be replaced by a newer implementation.
- Existing code should **not** be physically deleted.

Knowledge is considered more valuable than a small reduction in repository size.

---

# Future Cleanup

A dedicated repository cleanup will be performed **after the export architecture reaches a stable v1.0 release**.

Only then will deprecated files be evaluated for permanent removal.

The cleanup will be based on evidence rather than assumptions.

---

# Contributor Guidelines

When working on the export subsystem:

- Modify only the active pipeline.
- Do not restore dependencies on deprecated files.
- Do not delete deprecated files.
- Do not modernize deprecated implementations.
- Treat them as historical documentation.

---

# Design Principle

The repository favors preserving engineering knowledge over aggressive cleanup.

Temporary duplication is acceptable during architectural migration.

Permanent deletion is postponed until the new architecture has demonstrated long-term stability.