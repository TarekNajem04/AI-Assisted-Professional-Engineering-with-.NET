# Parameters Reference

## Export-Manuscript.ps1

The main entry point for the export pipeline.

### `-SourceFile` (Mandatory)

Path to the Markdown source file to export. Accepts both absolute and
relative paths (relative to the repository root).

```powershell
-SourceFile "book/chapters/Chapter-01/assembled/chapter-01.en.md"
```

The file name determines both the language (`en` or `ar` suffix) and the
document category via the source layout rules in `export-config.json`.

### `-StyleProfile`

The visual style profile to apply. Default: `"oreilly"`.

```powershell
-StyleProfile "default"
-StyleProfile "oreilly"
-StyleProfile "apress"
```

Each profile defines its own DOCX reference template (fonts, colors, spacing)
and PDF theme (layout, headers, typography). Profiles live under
`scripts/export/styles/<profile-name>/`.

### `-PdfEngine`

The PDF engine passed to Pandoc's `--pdf-engine` flag. Default: `"lualatex"`.

```powershell
-PdfEngine "lualatex"
```

Currently only `lualatex` is supported. The parameter is maintained for
future extensibility — adding a new engine requires updating the
`PdfEngines` array in `export-config.json` and the `ValidateSet` in this
parameter.

### `-RebuildMermaid`

Force regeneration of all Mermaid diagrams even if cached. Accepts `$true`
or `$false`. When `$null` (default), the config value `Mermaid.Rebuild` is
used.

```powershell
-RebuildMermaid $true
```

### `-RebuildCovers`

Force regeneration of cover images even if cached. Accepts `$true` or
`$false`. When `$null` (default), the config value `Covers.Rebuild` is used.

```powershell
-RebuildCovers $true
```

## Initialize-ExportEnvironment.ps1

Manages the `exports/` directory structure and placeholder files.

### `-Force`

Skip the interactive confirmation prompt. Full reset: deletes all output
files and recreates the folder structure with placeholder files.

```powershell
-Force
```

### `-CleanPlaceholdersOnly`

Non-destructive mode that removes `placeholder.txt` only from folders that
contain real output files. Creates missing placeholders in empty folders.
This is the mode used by the pipeline during normal export.

```powershell
-CleanPlaceholdersOnly
```

## export-config.json

The central configuration file at `scripts/export/export-config.json`.

| Key | Type | Description |
|-----|------|-------------|
| `ValidLanguages` | `string[]` | Supported languages: `["ar", "en"]` |
| `ValidTypes` | `string[]` | Output types: `["pdf", "docx"]` |
| `ValidCategories` | `string[]` | Document categories: `["chapters", "sections", "manifesto", "tests"]` |
| `PdfEngines` | `string[]` | Registered PDF engines: `["lualatex"]` |
| `OutputRoot` | `string` | Root output directory: `"exports"` |
| `Pdf.Engine` | `string` | Default PDF engine: `"lualatex"` |
| `Pdf.FontSize` | `string` | Base font size: `"12pt"` |
| `Pdf.PageSize` | `string` | Page size: `"a4"` |
| `Pdf.Margins` | `object` | Page margins (Top, Bottom, Inner, Outer, BindingOffset) |
| `Pdf.Fonts` | `object` | Font definitions (MainEn, MainAr, Sans, Mono) |
| `Pdf.LineHeight` | `number` | Line height multiplier: `1.05` |
| `Pdf.SyntaxHighlighting` | `string` | Syntax highlighting style: `"pygments"` |
| `Pdf.CoverPages` | `bool` | Include cover pages in PDF: `true` |
| `Pdf.Title` | `string` | Document title for metadata |
| `Docx.SyntaxHighlighting` | `string` | DOCX highlighting style: `"pygments"` |
| `Docx.ReferenceDoc` | `string` | Reference DOCX template file name |
| `Docx.CoverPages` | `bool` | Include cover pages in DOCX: `true` |
| `Docx.Margins` | `object` | DOCX margins (Top, Bottom, Left, Right) in cm |
| `Covers` | `object` | Cover image templates, dimensions, and quality settings |
| `Mermaid` | `object` | Mermaid diagram settings (width, background, cache dir) |
| `Export.CleanOutput` | `bool` | Delete existing output before export: `true` |
| `Export.Cache` | `object` | Cache directory configuration |
| `Log.Colors` | `object` | Console log color mapping |
