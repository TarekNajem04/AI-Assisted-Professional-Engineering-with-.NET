# PDF Engines

## Current Engine

The pipeline uses **LuaLaTeX** as its single PDF engine. It is invoked via
Pandoc's `--pdf-engine` flag.

```txt
--pdf-engine=lualatex
```

LuaLaTeX was chosen for:

- Full OpenType font support via `fontspec` (both Arabic and Latin scripts).
- `polyglossia` for multilingual typesetting and bidi support.
- Mature Pandoc integration.
- Consistent, professional-grade PDF typography.

## How the Engine Setting Flows

1. **Config**: `export-config.json` defines `PdfEngines: ["lualatex"]` and
   the default `Pdf.Engine: "lualatex"`.

2. **Context creation**: `Create-ApplicationContext.ps1` reads the config
   default and validates the engine against the `PdfEngines` list.

3. **Argument builder**: `Get-PandocPdfArguments.ps1` reads
   `$Context.PdfEngine` and passes it to Pandoc:

   ```powershell
   $arguments += "--pdf-engine=$($Context.PdfEngine)"
   ```

4. **Invocation**: `Invoke-Pandoc.ps1` calls Pandoc with the assembled
   arguments, including the chosen engine.

## How to Add a New PDF Engine

The architecture is designed for extensibility. To add a new engine:

### Step 1: Register the Engine

Add the engine name to the `PdfEngines` array in `export-config.json`:

```json
"PdfEngines": ["lualatex", "context"]
```

Update the `ValidateSet` in `Export-Manuscript.ps1`:

```powershell
[ValidateSet('lualatex', 'context')]
[string]$PdfEngine = 'lualatex',
```

### Step 2: Create an Argument Builder

Create `scripts/export/pandoc/Get-PandocPdfArguments.ps1` or extend the
existing one with a conditional branch for the new engine.

### Step 3: Update the PDF Dispatch

The engine name stored in `$Context.PdfEngine` is passed generically to
`Get-PandocPdfArguments.ps1`, which already uses it dynamically via
`--pdf-engine=$($Context.PdfEngine)`. If the new engine requires a different
set of flags beyond what the generic argument builder supports, add a
conditional dispatch in `Invoke-Pdf.ps1`.

### Step 4: Test

```powershell
pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
    -SourceFile "book/chapters/Chapter-01/assembled/chapter-01.en.md" `
    -PdfEngine "context"
```

## Legacy Engine: wkhtmltopdf

The file `Get-PandocWkHtmlToPdfArguments.ps1` exists in the `pandoc/`
directory as a historical reference. It is marked **DEPRECATED** and is not
part of the active pipeline. It is kept for architectural review and will
not be removed unless an explicit project decision is made.
