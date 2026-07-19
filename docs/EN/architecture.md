# Architecture

## Pipeline Overview

The export pipeline is a modular, stage-based system that processes Markdown
source files through 5 stages:

```txt
[1/5] Initialize Context
[2/5] Run Preprocessors
[3/5] Generate PDF
[4/5] Generate DOCX
[5/5] Clean up
```

Each stage is independent, and the context object (`ApplicationContext`) is
passed between them to carry configuration, state, and file paths.

## Directory Structure

```txt
scripts/export/
├── Export-Manuscript.ps1          # Main entry point
├── export-config.json             # Central configuration
│
├── core/                          # Core pipeline orchestration
│   ├── ApplicationContext.ps1     # Data class for export state
│   ├── Create-ApplicationContext.ps1
│   ├── Export-TargetResolver.ps1  # Language & category detection
│   ├── Invoke-Preprocessor.ps1    # Stage 2 orchestrator
│   ├── Invoke-Pdf.ps1             # Stage 3 orchestrator
│   ├── Invoke-Docx.ps1            # Stage 4 orchestrator
│   ├── Invoke-Pandoc.ps1          # Pandoc invocation wrapper
│   ├── Invoke-StyleProfile.ps1    # Style profile loader
│   └── Validate-DocxStyles.ps1    # DOCX style validation
│
├── pandoc/                        # Pandoc argument builders
│   ├── Get-PandocCommonArguments.ps1
│   ├── Get-PandocPdfArguments.ps1
│   └── Get-PandocDocxArguments.ps1
│
├── preprocessors/                 # Pre-processing scripts
│   ├── MarkdownPreprocessor.ps1
│   ├── MermaidPreprocessor.ps1
│   ├── CSharpPreprocessor.ps1
│   └── Invoke-CoverBuilder.ps1
│
├── project/                       # Project-specific knowledge
│   ├── Project-Conventions.ps1    # Language & kind detection
│   ├── Project-Tools.ps1          # External tool discovery
│   └── Get-ProjectPandocConfiguration.ps1
│
├── shared/                        # Shared utilities
│   ├── Validation.ps1             # Context validation
│   ├── Workspace.ps1              # Temp directory management
│   └── Write-Log.ps1              # Coloured logging
│
├── styles/                        # Visual style profiles
│   ├── default/                   # Default professional style
│   ├── oreilly/                   # O'Reilly-inspired style
│   └── apress/                    # Apress-inspired style
│
├── latex/                         # LaTeX templates
│   ├── pdf-theme.tex              # Base PDF theme template
│   └── arabic-setup.tex           # Arabic bidi configuration
│
├── lua/                           # Pandoc Lua filters
│   ├── codeblock-bidi.lua         # RTL/LTR code block handling
│   └── escape-backslash.lua       # Backslash escaping for LaTeX
│
├── tools/                         # Utility scripts
│   └── Initialize-ExportEnvironment.ps1
│
└── *.py                           # Python post-processing scripts
```

## Key Files Explained

### Export-Manuscript.ps1

The single entry point. It orchestrates all 5 stages:

1. Import all required modules
2. Create the `ApplicationContext` (config, paths, tools)
3. Apply the requested style profile
4. Validate the DOCX template
5. Clean previous output if configured
6. Run preprocessors (Mermaid, C#, covers)
7. Generate PDF via Pandoc + LuaLaTeX
8. Generate DOCX via Pandoc + Python post-processing
9. Clean up temporary workspace

Parameters: `-SourceFile`, `-StyleProfile`, `-PdfEngine`,
`-RebuildMermaid`, `-RebuildCovers`.

### Initialize-ExportEnvironment.ps1

Manages the `exports/` directory tree. It creates placeholder files in empty
directories (to keep them in Git) and removes them when real output files
appear. Supports two modes:

- **Full Reset** (`-Force`): delete all output files, keep placeholders.
- **Clean Placeholders** (`-CleanPlaceholdersOnly`): remove `placeholder.txt`
  only from folders that contain real files.

### generate-docx-style.py (per profile)

Python script that creates a professionally styled DOCX reference template
using `python-docx`. Each profile has its own, defining:

- Font families for body, headings, code, and captions
- Font sizes and colours
- Paragraph spacing and indentation
- Heading numbering styles
- Code block background colours

Output: `scripts/export/custom-reference.docx`

### generate-pdf-style.py (per profile)

Python script that generates a profile-specific PDF theme file (LaTeX) from
a base template. Customises:

- Font families via `fontspec`
- Page geometry via `geometry` package
- Heading styles
- Code block formatting
- Table of contents layout
- Header/footer design

Output: a `pdf-theme.tex` in the working directory.

### pdf-theme.tex

The base LaTeX template for PDF output. It is a Pandoc-include header that
sets up:

- Document class and page geometry
- Font selection with `fontspec` for OpenType support
- Polyglossia language configuration
- Code block styling with `tcolorbox` or `minted`
- Table of contents formatting
- Header/footer rules
- Hyperlink configuration

Each style profile has its own `pdf-theme.tex` that extends or overrides
the base template.

### arabic-setup.tex

LaTeX header included when exporting Arabic-language documents. It
configures:

- `polyglossia` with Arabic as the main language
- Right-to-left text direction
- Arabic font families (`Traditional Arabic`)
- Bidi-related adjustments for lists, tables, and code blocks
- Date and number localisation

### codeblock-bidi.lua

A Pandoc Lua filter that normalises code blocks for bidirectional text
handling across output formats:

- **PDF/LaTeX**: wraps code blocks in `\begin{flushleft}\textdir TLT...`
  to force left-to-right rendering inside RTL documents.
- **DOCX**: sets the `direction="ltr"` attribute on code blocks.
- **HTML**: sets the `direction="ltr"` attribute on code blocks.

It also marks blocks containing Arabic characters with
`contains-arabic="true"` for downstream processing.

### escape-backslash.lua

A Pandoc Lua filter for LaTeX/PDF output that escapes backslashes in code
blocks to `\textbackslash{}`. This prevents Pandoc from interpreting C#
code like `namespace Foo.Bar` as LaTeX commands.
