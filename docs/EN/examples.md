# Examples

## Example 1: Export a Full Book Chapter (English, O'Reilly Style)

```powershell
pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
    -SourceFile "book/chapters/Chapter-01/assembled/chapter-01.en.md" `
    -StyleProfile "oreilly"
```

Output:

```txt
exports/pdf/en/chapters/chapter-01.en.pdf
exports/docx/en/chapters/chapter-01.en.docx
```

## Example 2: Export an Arabic Chapter (Default Style)

```powershell
pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
    -SourceFile "book/chapters/Chapter-01/assembled/chapter-01.ar.md" `
    -StyleProfile "default"
```

Arabic output includes the `arabic-setup.tex` header for proper RTL
typesetting with `polyglossia`.

## Example 3: Export with Forced Mermaid Rebuild

```powershell
pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
    -SourceFile "book/chapters/Chapter-01/assembled/chapter-01.en.md" `
    -StyleProfile "apress" `
    -RebuildMermaid $true
```

This regenerates all Mermaid diagram PNGs even if they exist in the cache.

## Example 4: Export Multiple Chapters

Loop through all English assembled chapters:

```powershell
$files = Get-ChildItem "book/**/assembled/*.en.md" -Recurse
foreach ($file in $files) {
    pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
        -SourceFile $file.FullName `
        -StyleProfile "oreilly"
}
```

## Example 5: Export a Section Manuscript

```powershell
pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
    -SourceFile "book/sections/introduction/assembled/introduction.en.md" `
    -StyleProfile "oreilly"
```

Output lands in `exports/pdf/en/sections/` and `exports/docx/en/sections/`.

## Example 6: Full Reset of Export Directory

```powershell
pwsh -ExecutionPolicy Bypass -File scripts\export\tools\Initialize-ExportEnvironment.ps1 -Force
```

This deletes all existing PDF, DOCX, diagram, and cover files while keeping
the directory structure and placeholder files intact.

## Example 7: Check Output Files

After export, verify the files:

```powershell
Get-ChildItem exports/pdf -Recurse -Filter *.pdf
Get-ChildItem exports/docx -Recurse -Filter *.docx
```

## Example 8: Running with Clean Environment

Full pipeline with environment reset before export:

```powershell
pwsh -ExecutionPolicy Bypass -File scripts\export\tools\Initialize-ExportEnvironment.ps1 -Force
pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
    -SourceFile "book/chapters/Chapter-01/assembled/chapter-01.en.md" `
    -StyleProfile "oreilly"
```

## Example 9: DOCX Post-processing Breakdown

When generating DOCX, the pipeline runs these post-processing steps:

1. Normalise highlight colours (via `docx-apply-hl-colors.py`)
2. Fit images to page dimensions (via `docx-fit-images.py`)
3. Set document margins from config (via `docx-set-margins.py`)
4. Add front/back cover images (via `docx-add-covers.py`)

Each step runs only if the corresponding Python script exists.

## Example 10: PDF Output with LuaLaTeX

The PDF generation uses these LuaLaTeX features:

| Feature        | Implementation                                                   |
|----------------|------------------------------------------------------------------|
| OpenType fonts | `fontspec` package with `\setmainfont`, `\sansfont`, `\monofont` |
| Arabic text    | `polyglossia` with `\setmainlanguage[locale=morocco]{arabic}`    |
| Code blocks    | `tcolorbox` or `minted` for syntax-highlighted code              |
| Bidi code      | `\textdir TLT` in code blocks via Lua filter                     |
| Page geometry  | `geometry` package with binding offset for book layout           |
