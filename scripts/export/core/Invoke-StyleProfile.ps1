<#
    SPDX-License-Identifier: MIT
    Copyright (c) 2026 Tarek Najem

    This file is part of the AI-Assisted Professional Engineering with .NET
    book project (https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET).
    It is subject to the terms and conditions of the MIT License as published
    in the LICENSE file at the root of this repository.

    This header must not be removed or modified without preserving the
    LICENSE file reference and copyright notice.

    Description:
        Load a style profile, run its DOCX and PDF generators, and update
        the export context with generated files.
#>
Set-StrictMode -Version Latest

function Invoke-StyleProfile {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory)]
    [object]$Context,
    [Parameter(Mandatory)]
    [string]$ProfileName
  )

  $profileDir = Join-Path $Context.Paths.StylesDir $ProfileName
  if (-not (Test-Path $profileDir)) {
    throw "Style profile '$ProfileName' not found at: $profileDir"
  }

  # Log profile switch
  $oldProfile = $Context.StyleProfileName
  if ($oldProfile -and $oldProfile -ne $ProfileName) {
    Write-Log "  [PROFILE] Switching: '$oldProfile' → '$ProfileName'" Highlight
  }

  $profileJson = Join-Path $profileDir "profile.json"
  if (-not (Test-Path $profileJson)) {
    throw "profile.json missing for style profile '$ProfileName'"
  }

  $profile = Get-Content -LiteralPath $profileJson -Raw -Encoding UTF8 | ConvertFrom-Json
  $configPath = $Context.ConfigPath
  $projectRoot = $Context.RepositoryRoot

  # Store profile-specific syntax highlighting
  # If it's a file path (has dir separator or extension), resolve absolutely.
  # Otherwise treat it as a built-in style name (breezedark, pygments, etc.).
  if ($null -ne $profile.docx.PSObject.Properties['syntaxHighlighting'] -and $profile.docx.syntaxHighlighting) {
    $hlPath = $profile.docx.syntaxHighlighting
    $hasPathChars = $hlPath.Contains('\') -or $hlPath.Contains('/') -or $hlPath.Contains('.')
    if ($hasPathChars -and -not [System.IO.Path]::IsPathRooted($hlPath)) {
      $hlPath = Join-Path $Context.RepositoryRoot $hlPath
    }
    $Context.DocxSyntaxHighlighting = $hlPath
    Write-Log "  [PROFILE] Highlight: $hlPath" Muted
  }

  # ---- DOCX generator ----
  $docxGen = Join-Path $profileDir $profile.docx.generator
  if (Test-Path $docxGen) {
    Write-Log "  [PROFILE] Generator: $(Split-Path $docxGen -Leaf)" Muted
    Write-Log "  [PROFILE]   Template : $(ConvertTo-RelativePath $Context.TemplateFilePath)" Muted
    if (Test-Path $Context.TemplateFilePath) {
      Write-Log "  [PROFILE]   Removing : $(ConvertTo-RelativePath $Context.TemplateFilePath)" Deleted
      Remove-Item -LiteralPath $Context.TemplateFilePath -Force
    }
    & python $docxGen --output "$($Context.TemplateFilePath)" --config "$configPath" --project-root "$projectRoot" 2>&1 | ForEach-Object { Write-Log $_ Gray }
    if ($LASTEXITCODE -ne 0) { throw "DOCX generator failed for profile '$ProfileName' (exit $LASTEXITCODE)" }
    Write-Log "  [PROFILE]   → Created: $(ConvertTo-RelativePath $Context.TemplateFilePath)" Success
  }
  else {
    Write-Log "  [WARN] DOCX generator not found: $(ConvertTo-RelativePath $docxGen)" Warning
  }

  # ---- PDF generator ----
  $pdfGen = Join-Path $profileDir $profile.pdf.generator
  if (Test-Path $pdfGen) {
    $profileTheme = Join-Path $profileDir "pdf-theme.tex"
    if (-not (Test-Path $profileTheme)) { $profileTheme = $Context.Paths.ThemeFile }
    $outputTheme = Join-Path $Context.Working.Directory "pdf-theme.tex"
    Write-Log "  [PROFILE] Generator: $(Split-Path $pdfGen -Leaf)" Muted
    Write-Log "  [PROFILE]   Theme   : $(ConvertTo-RelativePath $profileTheme)" Muted
    & python $pdfGen --output "$outputTheme" --template "$profileTheme" --config "$configPath" --project-root "$projectRoot" 2>&1 | ForEach-Object { Write-Log $_ Gray }
    if ($LASTEXITCODE -ne 0) { throw "PDF generator failed for profile '$ProfileName' (exit $LASTEXITCODE)" }

    $Context.Working.ThemeFile = $outputTheme
    Write-Log "  [PROFILE]   → Created: pdf-theme.tex" Success
  }
  else {
    Write-Log "  [WARN] PDF generator not found: $(ConvertTo-RelativePath $pdfGen)" Warning
  }

  $Context.StyleProfileName = $ProfileName
  Write-Log "  [PROFILE] Style profile '$ProfileName' applied." Success
}
