# Style Profiles

## Overview

Style profiles are self-contained sets of templates and generators that
define the visual appearance of exported PDF and DOCX documents. Each
profile lives in its own directory under `scripts/export/styles/`.

## Built-in Profiles

| Profile | Body Font | Heading Font | Code Font | Characteristic |
|---------|-----------|--------------|-----------|----------------|
| `default` | EB Garamond | Source Sans Pro | Consolas | Classic professional book |
| `oreilly` | Noto Serif | Open Sans | Consolas | Dark code blocks, blue accents |
| `apress` | Palatino Linotype | Segoe UI | Consolas | Conservative grayscale |

## Profile Directory Structure

Each profile follows this structure:

```txt
styles/<profile-name>/
├── profile.json              # Profile metadata and generator references
├── pdf-theme.tex             # LaTeX theme template (PDF output)
├── generate-docx-style.py    # DOCX reference document generator
├── generate-pdf-style.py     # PDF theme generator
└── docx-validate-styles.py   # DOCX style validation script
```

## profile.json

The profile manifest file. Example (`oreilly/profile.json`):

```json
{
  "name": "oreilly",
  "description": "O'Reilly-inspired professional book style — Noto Serif body, Open Sans headings, dark code blocks, deep blue accents",
  "version": "1.0",
  "docx": {
    "generator": "generate-docx-style.py",
    "syntaxHighlighting": "breezedark"
  },
  "pdf": {
    "generator": "generate-pdf-style.py"
  }
}
```

Fields:

- `name`: profile identifier matching the directory name.
- `description`: human-readable summary.
- `version`: profile schema version.
- `docx.generator`: Python script to create the DOCX reference template.
- `docx.syntaxHighlighting`: syntax highlighting style (e.g., `pygments`,
  `breezedark`, or a file path).
- `pdf.generator`: Python script to generate the PDF theme file.

## How Profiles Are Applied

When `Invoke-StyleProfile.ps1` runs:

1. Loads `profile.json` from the profile directory.
2. Runs `generate-docx-style.py` → produces `custom-reference.docx`.
3. Runs `generate-pdf-style.py` → produces `pdf-theme.tex` in the working
   directory.
4. Updates `Context.StyleProfileName` and `Context.DocxSyntaxHighlighting`.

The generated files are then used by `Get-PandocDocxArguments.ps1` (for the
`--reference-doc` flag) and `Get-PandocPdfArguments.ps1` (for the header
include of the theme).

## How to Add a New Style Profile

1. Create a new directory: `styles/<new-profile>/`

2. Copy the files from an existing profile (e.g., `default/`).

3. Edit `profile.json`:
   - Set `name` to your profile name.
   - Update the `description`.
   - Adjust `docx.syntaxHighlighting` if needed (e.g., `"monochrome"`).

4. Edit `generate-docx-style.py`:
   - Change font families, sizes, and colours.
   - Modify paragraph spacing and indentation.
   - Adjust heading styles and numbering.
   - Update code block styling.

5. Edit `generate-pdf-style.py`:
   - Change font families via `\setmainfont`, `\sansfont`, `\monofont`.
   - Adjust page geometry and margins.
   - Modify heading formatting and colours.
   - Update code block appearance.

6. Edit `pdf-theme.tex` if you need further LaTeX customisation:
   - Header/footer content.
   - Table of contents layout.
   - Page numbering style.
   - Caption formatting.

7. Test the profile:

   ```powershell
   pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
       -SourceFile "book/chapters/Chapter-01/assembled/chapter-01.en.md" `
       -StyleProfile "new-profile"
   ```
