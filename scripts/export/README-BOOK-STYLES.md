# Book DOCX Styling System

This directory contains a complete professional DOCX styling system for
the book *AI-Assisted Professional Engineering with .NET*.

The system follows the O'Reilly/Manning typographic model:
serif body text (EB Garamond), sans-serif headings (Source Sans Pro),
and monospace code (Consolas).

## Files

| File                         | Purpose                                                        |
|------------------------------|----------------------------------------------------------------|
| `generate-reference-docx.py` | Generates `custom-reference.docx` from scratch                 |
| `custom-reference.docx`      | Pandoc reference DOCX with all book styles embedded            |
| `docx-validate-styles.py`    | Validates that all required styles exist in the reference DOCX |
| `export-config.json`         | Central configuration for the export pipeline                  |
| `README-BOOK-STYLES.md`      | This file                                                      |

## Style Inventory

### Paragraph Styles

| Style Name       | Font            | Size | Usage                  |
|------------------|-----------------|------|------------------------|
| BookBody         | EB Garamond     | 12pt | Body text              |
| BookHeading1     | Source Sans Pro | 28pt | Chapter titles         |
| BookHeading2     | Source Sans Pro | 18pt | Section headings       |
| BookHeading3     | Source Sans Pro | 14pt | Subsection headings    |
| BookCode         | Consolas        | 9pt  | Code blocks            |
| BookQuote        | EB Garamond     | 12pt | Block quotations       |
| BookTable        | EB Garamond     | 10pt | Table content          |
| BookCaption      | Source Sans Pro | 10pt | Table captions         |
| BookImageCaption | Source Sans Pro | 10pt | Figure captions        |
| BookDate         | EB Garamond     | 11pt | Date/byline            |
| BookTOC1         | Source Sans Pro | 12pt | TOC chapter entries    |
| BookTOC2         | Source Sans Pro | 11pt | TOC section entries    |
| BookTOC3         | Source Sans Pro | 10pt | TOC subsection entries |

### Character Styles

| Style Name     | Font     | Size  | Usage                  |
|----------------|----------|-------|------------------------|
| BookInlineCode | Consolas | 9.5pt | Inline code references |

## Usage

### 1. Generate the reference DOCX

```bash
pip install python-docx
python scripts/export/generate-reference-docx.py
```

Optional: pass a custom config path:

```bash
python scripts/export/generate-reference-docx.py --config /path/to/export-config.json
```

### 2. Validate styles

```bash
python scripts/export/docx-validate-styles.py
```

Exit code 0 = all styles present and correct.
Exit code 1 = one or more styles missing or incorrect.

### 3. Use with Pandoc

```bash
pandoc manuscript.md \
  --reference-doc=scripts/export/custom-reference.docx \
  --to=docx \
  -o output.docx
```

## Font Notes

- **EB Garamond** — Serif body font. Available as open source (SIL OFL).
- **Source Sans Pro** — Sans-serif heading font. Available as open source (SIL OFL).
- **Consolas** — Monospace code font. Ships with Windows and Visual Studio.

Readers who do not have EB Garamond or Source Sans Pro installed will
see a fallback font (typically Times New Roman or Calibri). For best
results, instruct readers to install the fonts before opening the DOCX.

## Architecture

The DOCX pipeline works as follows:

1. `generate-reference-docx.py` creates a `.docx` file containing all
   style definitions, sample content, and page layout settings.
2. Pandoc reads this reference DOCX via `--reference-doc` and applies
   its styles to the converted output.
3. `docx-validate-styles.py` provides a CI-checkable validation step to
   catch missing or misconfigured styles before they reach readers.

All style definitions are also mirrored in `export-config.json` under
`docx.styles` for pipeline-level configuration.
