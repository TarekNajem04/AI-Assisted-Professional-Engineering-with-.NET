<#
.NOTES
    SPDX-License-Identifier: MIT
    Copyright (c) 2026 Tarek Najem

    This file is part of the AI-Assisted Professional Engineering with .NET
    book project (https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET).
    It is subject to the terms and conditions of the MIT License as published
    in the LICENSE file at the root of this repository.

    This header must not be removed or modified without preserving the
    LICENSE file reference and copyright notice.

.SYNOPSIS
    Converts a source file to PDF AND DOCX using a specified engine.

.DESCRIPTION
    Processes a source file and converts it to PDF/DOCX format with options to rebuild Mermaid diagrams and book covers.
    Currently supports only the 'lualatex' engine.

.PARAMETER SourceFile
    The full path to the source file to be converted. (Mandatory)

.PARAMETER PdfEngine
    The engine used to generate the PDF.
    Default: 'lualatex'
    Allowed values: 'lualatex'

.PARAMETER RebuildMermaid
    Specifies whether to regenerate Mermaid diagrams.
    - $true: Force rebuild.
    - $false: Do not rebuild.
    - $null: Use system default behavior.

.PARAMETER RebuildCovers
    Specifies whether to regenerate book covers.
    - $true: Force rebuild.
    - $false: Do not rebuild.
    - $null: Use system default behavior.

.PARAMETER StyleProfile
    The name of the style profile used for formatting the output.
    Default: 'oreilly'

.EXAMPLE
    .\scripts\export\Export-Manuscript.ps1 -SourceFile "C:\Docs\book.md" -StyleProfile "apress"
    Converts the file using a custom style profile.

.EXAMPLE
    .\scripts\export\Export-Manuscript.ps1 -SourceFile "input.md" -RebuildMermaid $true
    Converts the file and forces rebuilding of diagrams.

    .\scripts\export\Export-Manuscript.ps1 -SourceFile "book\manifesto\introduction.{LNG}.md" -PdfEngine lualatex -RebuildMermaid $false -RebuildCovers $false -StyleProfile oreilly
    .\scripts\export\Export-Manuscript.ps1 -SourceFile "book\chapters\Chapter-{INDX}\sections\section-{INDEX}.{LNG}.md" -PdfEngine lualatex -RebuildMermaid $false -RebuildCovers $false -StyleProfile oreilly
    .\scripts\export\Export-Manuscript.ps1 -SourceFile "book\chapters\Chapter-{INDX}\assembled\chapter-{INDEX}.{LNG}.md" -PdfEngine lualatex -RebuildMermaid $false -RebuildCovers $false -StyleProfile oreilly

#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$SourceFile,
    [ValidateSet('lualatex')]
    [string]$PdfEngine = 'lualatex',
    [System.Nullable[bool]] $RebuildMermaid = $null,
    [System.Nullable[bool]] $RebuildCovers = $null,
    [string]$StyleProfile = "oreilly"
)

$ProjectRoot = Convert-Path ((git rev-parse --show-toplevel).Trim())

$Imports = @(
    "scripts\export\core\ApplicationContext.ps1",
    "scripts\export\core\Create-ApplicationContext.ps1",
    "scripts\export\shared\Validation.ps1",
    "scripts\export\core\Invoke-Preprocessor.ps1",
    "scripts\export\shared\Write-Log.ps1",
    "scripts\export\preprocessors\CSharpPreprocessor.ps1",
    "scripts\export\core\Invoke-Pdf.ps1",
    "scripts\export\core\Invoke-Docx.ps1",
    "scripts\export\project\Get-ProjectPandocConfiguration.ps1",
    "scripts\export\pandoc\Get-PandocDocxArguments.ps1",
    "scripts\export\pandoc\Get-PandocCommonArguments.ps1",
    "scripts\export\pandoc\Get-PandocPdfArguments.ps1",
    "scripts\export\shared\Workspace.ps1",
    "scripts\export\core\Invoke-Pandoc.ps1",
    "scripts\export\core\Validate-DocxStyles.ps1",
    "scripts\export\core\Invoke-StyleProfile.ps1"
)

foreach ($File in $Imports) {
    $Path = Join-Path $ProjectRoot $File
    if (Test-Path $Path) { . $Path }
    else { Write-Warning "Could not load component: $Path" }
}

function ConvertTo-RelativePath {
    param([string]$AbsolutePath)
    $relative = $AbsolutePath.Substring($ProjectRoot.Length).TrimStart('\')
    if ($relative.Length -eq 0) { return '.\' }
    return $relative
}

function Test-TemplateValid {
    param([object]$Context)

    if (-not (Test-Path $Context.TemplateFilePath)) {
        Write-Log "  [TEMPLATE] Template missing after profile generation: $(ConvertTo-RelativePath $Context.TemplateFilePath)" Warning
        return $false
    }

    $valid = Invoke-DocxStyleValidation -Context $Context
    if (-not $valid) {
        Write-Log "  [TEMPLATE] Template invalid — consider checking your style profile." Warning
        return $false
    }

    return $true
}

function Update-GitIgnore {
    param([object]$Context)

    $gitignorePath = Join-Path $Context.RepositoryRoot ".gitignore"
    if (-not (Test-Path $gitignorePath)) {
        Write-Log "  [GITIGNORE] .gitignore not found — skipping." Muted
        return
    }

    $content = Get-Content -LiteralPath $gitignorePath -Raw -Encoding UTF8
    $pattern = [regex]::Escape($Context.TemplateFileName)

    if ($content -notmatch $pattern) {
        $entry = "`r`n# Reference DOCX template (auto-generated)$([char]0x0d)$([char]0x0a)$($Context.TemplateFileName)"
        Add-Content -LiteralPath $gitignorePath -Value $entry -Encoding UTF8
        Write-Log "  [GITIGNORE] Added template file to .gitignore: $($Context.TemplateFileName)" Success
    }
    else {
        Write-Log "  [GITIGNORE] Template already listed in .gitignore." Muted
    }
}

if (-not $PdfEngine) { $PdfEngine = 'lualatex' }

Write-Log "---------------------------------------------------------------------------" Banner
Write-Log "  Manuscript Export System - Professional Book Pipeline" Banner
Write-Log "---------------------------------------------------------------------------" Banner

# Always clean placeholders before export (no prompt, only when real files exist)
try {
    $initScript = Join-Path $ProjectRoot "scripts\export\tools\Initialize-ExportEnvironment.ps1"
    if (Test-Path $initScript) {
        Write-Log "[Stage] Placeholder Cleanup" Section
        Write-Log "  Running Initialize-ExportEnvironment.ps1 (CleanPlaceholdersOnly)" Muted
        & powershell -ExecutionPolicy Bypass -File $initScript -Force -CleanPlaceholdersOnly
    }
    else {
        Write-Log "  [WARN] Initialize-ExportEnvironment.ps1 not found — skipping placeholder cleanup." Warning
    }
}
catch {
    Write-Warning "Placeholder cleanup failed: $($_.Exception.Message)"
}

try {
    Write-Log
    Write-Log '[1/5] Initializing Export Context...' Stage

    $workspace = New-Workspace
    $Context = Create-ApplicationContext `
        -SourceFile $SourceFile `
        -RepositoryRoot $ProjectRoot `
        -PdfEngine $PdfEngine `
        -Workspace $workspace

    if ($RebuildMermaid -ne $null) { $Context.Config.Mermaid.Rebuild = $RebuildMermaid }
    if ($RebuildCovers -ne $null) { $Context.Config.Covers.Rebuild = $RebuildCovers }

    Initialize-LogColors -Config $Context.Config

    Test-ExportContext -Context $Context
}
catch {
    Write-Error "Failed to initialize export context: $($_.Exception.Message)"
    return
}

Write-Log
Write-Log "[Stage] Style Profile" Section
try {
    $Context.StyleProfileName = $StyleProfile
    Invoke-StyleProfile -Context $Context -ProfileName $StyleProfile
}
catch {
    Write-Error "Style profile '$StyleProfile' failed: $($_.Exception.Message)"
    return
}

Write-Log
Write-Log "[Stage] Template Validation" Section
try {
    Test-TemplateValid -Context $Context
    Update-GitIgnore -Context $Context
}
catch {
    Write-Error "Template setup failed: $($_.Exception.Message)"
    return
}

Write-Log
Write-Log "[Stage] Output Cleanup" Section

$cleanOutput = $Context.Config.Export.CleanOutput
if ($cleanOutput) {
    if (Test-Path $Context.PdfOutputFile) {
        Remove-Item -LiteralPath $Context.PdfOutputFile -Force
        Write-Log "  PDF : Deleted $(ConvertTo-RelativePath $Context.PdfOutputFile)" Deleted
    }
    else {
        Write-Log "  PDF : No existing file" Muted
    }

    if (Test-Path $Context.DocxOutputFile) {
        Remove-Item -LiteralPath $Context.DocxOutputFile -Force
        Write-Log "  DOCX: Deleted $(ConvertTo-RelativePath $Context.DocxOutputFile)" Deleted
    }
    else {
        Write-Log "  DOCX: No existing file" Muted
    }
}
else {
    Write-Log "  Cache preserved — keeping existing output files" Muted
}

try {
    Write-Log
    Write-Log '[2/5] Running Preprocessors...' Stage
    Invoke-Preprocessor -Context $Context
}
catch {
    Write-Error "Preprocessing failed: $($_.Exception.Message)"
    return
}

try {
    Write-Log '[3/5] Generating PDF...' Stage
    Invoke-Pdf -Context $Context
}
catch {
    Write-Error "PDF generation failed: $($_.Exception.Message)"
}

try {
    Write-Log '[4/5] Generating DOCX...' Stage
    Invoke-Docx -Context $Context
}
catch {
    Write-Error "DOCX generation failed: $($_.Exception.Message)"
}

try {
    Write-Log '[5/5] Cleaning up...' Stage
    Remove-Workspace -Workspace $Context.Working.Directory
}
catch {
    Write-Warning "Cleanup failed: $($_.Exception.Message)"
}

Write-Log '---------------------------------------------------------------------------' Banner
Write-Log '  Export completed successfully!' Banner
Write-Log '---------------------------------------------------------------------------' Banner

