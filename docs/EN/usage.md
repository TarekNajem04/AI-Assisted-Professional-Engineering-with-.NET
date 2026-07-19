# Usage Guide

## Prerequisites

- **Pandoc** (>= 3.1)
- **LuaLaTeX** (TeX Live or MiKTeX with `fontspec`, `polyglossia`,
  `libertinus` packages)
- **Python 3** (for style generators and DOCX post-processing)
- **Node.js** (for Mermaid CLI: `@mermaid-js/mermaid-cli`)
- **ImageMagick** (for cover image processing)
- **PowerShell 7+** (`pwsh`)

## Quick Start

Export a single chapter with the default style profile:

```powershell
pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
    -SourceFile "book/chapters/Chapter-01/assembled/chapter-01.en.md"
```

## Full Export Workflow

### 1. Export a Single Chapter

English chapter with O'Reilly style:

```powershell
pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
    -SourceFile "book/chapters/Chapter-01/assembled/chapter-01.en.md" `
    -StyleProfile "oreilly"
```

Arabic chapter with Apress style:

```powershell
pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
    -SourceFile "book/chapters/Chapter-01/assembled/chapter-01.ar.md" `
    -StyleProfile "apress"
```

### 2. Controlling Cached Artifacts

Force rebuild of Mermaid diagrams:

```powershell
pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
    -SourceFile "book/chapters/Chapter-01/assembled/chapter-01.en.md" `
    -RebuildMermaid $true
```

Force rebuild of cover images:

```powershell
pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
    -SourceFile "book/chapters/Chapter-01/assembled/chapter-01.en.md" `
    -RebuildCovers $true
```

### 3. Selecting a Style Profile

Three profiles are available:

| Profile | Description |
|---------|-------------|
| `default` | Professional book style — EB Garamond body, Source Sans Pro headings |
| `oreilly` | O'Reilly-inspired — Noto Serif body, dark code blocks, blue accents |
| `apress` | Apress-inspired — traditional serif body, conservative grayscale palette |

```powershell
-StyleProfile "oreilly"
```

### 4. Output Structure

Generated files are placed under the `exports/` directory:

```txt
exports/
├── pdf/
│   ├── en/
│   │   └── chapters/
│   │       └── chapter-01.en.pdf
│   └── ar/
│       └── chapters/
│           └── chapter-01.ar.pdf
└── docx/
    ├── en/
    │   └── chapters/
    │       └── chapter-01.en.docx
    └── ar/
        └── chapters/
            └── chapter-01.ar.docx
```

## Silent / Non-Interactive Mode

The pipeline runs fully non-interactive by default. The `Initialize-ExportEnvironment.ps1`
script accepts `-Force -CleanPlaceholdersOnly` when called from the pipeline:

```powershell
pwsh -ExecutionPolicy Bypass -File scripts\export\tools\Initialize-ExportEnvironment.ps1 `
    -Force -CleanPlaceholdersOnly
```

## Environment Initialisation

To reset the export directory structure (delete all output files except
placeholders):

```powershell
pwsh -ExecutionPolicy Bypass -File scripts\export\tools\Initialize-ExportEnvironment.ps1 -Force
```
