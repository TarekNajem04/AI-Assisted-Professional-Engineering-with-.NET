# AI-Assisted Professional Engineering with .NET — Export Pipeline

## Overview

The Export Pipeline is a professional-grade manuscript generation system for
the book *AI-Assisted Professional Engineering with .NET*. It transforms
Markdown source files into publication-ready PDF and DOCX documents using
Pandoc as the core converter, LuaLaTeX for PDF typesetting, and Python
scripts for style generation and post-processing.

## Goals

- Produce high-quality PDF and DOCX outputs from a single Markdown source.
- Support bilingual documents (Arabic / English) with proper bidirectional
  text handling.
- Provide publisher-specific style profiles (Default, O'Reilly, Apress).
- Cache intermediate artifacts (Mermaid diagrams, cover images) for fast
  incremental builds.
- Clean, modular architecture that is easy to extend with new output formats
  or style profiles.

## Capabilities

| Feature | Description |
|---------|-------------|
| PDF generation | Professional PDF via Pandoc + LuaLaTeX with custom themes |
| DOCX generation | Styled DOCX via Pandoc with reference templates |
| Mermaid diagrams | Automatic rendering of Mermaid diagrams to PNG |
| Cover pages | Front/back covers from HTML templates with ImageMagick |
| C# code processing | Syntax-preserving C# code block handling |
| Bidirectional text | Full RTL/LTR support for Arabic/English mixed content |
| Style profiles | Three built-in profiles: Default, O'Reilly, Apress |
| Incremental builds | Cached diagrams and covers skip regeneration |
| Temporary workspace | Isolated temp directory per export, auto-cleaned |

## Why This Pipeline?

Existing tools like PrinceXML, Typst, or web-based converters were evaluated
but lacked the combination of features needed:

- **Pandoc** provides the most mature Markdown-to-everything conversion with
  Lua filter extensibility.
- **LuaLaTeX** delivers professional-grade PDF typography with `fontspec`
  for OpenType font support across Arabic and Latin scripts.
- **Python scripting** handles DOCX post-processing (margins, covers,
  syntax highlighting normalization) that Pandoc alone cannot.
- The modular PowerShell-based pipeline is designed for maintainability and
  extension without deep knowledge of every component.
