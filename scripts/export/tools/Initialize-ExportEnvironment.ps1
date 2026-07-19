<#
    SPDX-License-Identifier: MIT
    Copyright (c) 2026 Tarek Najem

    This file is part of the AI-Assisted Professional Engineering with .NET
    book project (https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET).
    It is subject to the terms and conditions of the MIT License as published
    in the LICENSE file at the root of this repository.

    This header must not be removed or modified without preserving the
    LICENSE file reference and copyright notice.
#>

<#
.SYNOPSIS
    Initializes the export environment — manages folder structure and placeholder files.

.DESCRIPTION
    This script manages the folder structure under the `exports` directory.
    It ensures that every folder always contains a `placeholder.txt` file when empty.
    It never deletes folders, only files.

    Full Reset Mode:
      - Deletes all files inside each folder EXCEPT placeholder.txt.
      - Ensures placeholder.txt exists in every folder.
      - Does not delete any folders.

    CleanPlaceholdersOnly Mode:
      - Deletes placeholder.txt only when real files exist in the same folder.
      - If a folder contains only placeholder.txt, nothing is deleted.
      - If a folder is empty and missing placeholder.txt, it is created.

.PARAMETER Force
    Skips the confirmation prompt in Full Reset mode.

.PARAMETER CleanPlaceholdersOnly
    Run in CleanPlaceholdersOnly mode (no structural changes, no prompt).

.EXAMPLE
    # Full reset with interactive confirmation
    powershell -File scripts\export\tools\Initialize-ExportEnvironment.ps1

.EXAMPLE
    # Full reset without confirmation
    powershell -File scripts\export\tools\Initialize-ExportEnvironment.ps1 -Force

.EXAMPLE
    # Only clean placeholders where real files exist
    powershell -File scripts\export\tools\Initialize-ExportEnvironment.ps1 -CleanPlaceholdersOnly

.EXAMPLE
    # Scripted call from Export-Manuscript (no prompt, clean only)
    powershell -File scripts\export\tools\Initialize-ExportEnvironment.ps1 -Force -CleanPlaceholdersOnly
#>

param(
  [switch] $Force,
  [switch] $CleanPlaceholdersOnly
)

$ScriptPath = $MyInvocation.MyCommand.Path
$ScriptDir = Split-Path $ScriptPath -Parent
$ProjectRoot = (Get-Item $ScriptDir).Parent.Parent.Parent.FullName | Convert-Path
$ExportsRoot = Join-Path $ProjectRoot "exports"

Write-Host ""
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host " INITIALIZING EXPORT ENVIRONMENT" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "Project root : $ProjectRoot"
Write-Host "Exports root : $ExportsRoot"
Write-Host ""

$placeholderContent = @"
This placeholder file exists only to allow this folder to be committed
to the repository. It will be automatically deleted when the export
pipeline starts generating real output files.
"@

function ConvertTo-RelativePath {
  param([string]$AbsolutePath)
  $relative = $AbsolutePath.Substring($ProjectRoot.Length).TrimStart('\')
  if ($relative.Length -eq 0) { return '.\' }
  return '~\' + $relative
}

if (-not $Force -and -not $CleanPlaceholdersOnly) {
  Write-Host "⚠ WARNING: This will delete ALL files except placeholder.txt in exports folders." -ForegroundColor Red
  Write-Host "Do you want to continue? (Y/N)" -ForegroundColor Yellow
  $answer = Read-Host
  if ($answer -ne "Y" -and $answer -ne "y") {
    Write-Host "Operation cancelled." -ForegroundColor DarkYellow
    return
  }
}

$structure = @(
  ".",  # exports root
  "covers",
  "diagrams\mermaid",
  "docx\ar\chapters", "docx\ar\sections", "docx\ar\manifesto", "docx\ar\tests",
  "docx\en\chapters", "docx\en\sections", "docx\en\manifesto", "docx\en\tests",
  "pdf\ar\chapters", "pdf\ar\sections", "pdf\ar\manifesto", "pdf\ar\tests",
  "pdf\en\chapters", "pdf\en\sections", "pdf\en\manifesto", "pdf\en\tests"
)

if (-not $CleanPlaceholdersOnly) {
  Write-Host "Mode: Full reset" -ForegroundColor DarkCyan
}
else {
  Write-Host "Mode: CleanPlaceholdersOnly" -ForegroundColor DarkCyan
}

foreach ($rel in $structure) {
  $path = Join-Path $ExportsRoot $rel
  $placeholderPath = Join-Path $path "placeholder.txt"

  if (-not (Test-Path $path)) {
    New-Item -ItemType Directory -Force -Path $path | Out-Null
    Write-Host ("Created: {0}" -f (ConvertTo-RelativePath $path)) -ForegroundColor Gray
  }

  $realFiles = Get-ChildItem -Path $path -File -Force -ErrorAction SilentlyContinue  | Where-Object { $_.Name -ne "placeholder.txt" }

  if ( ($realFiles.Count -eq 0) -and (-not (Test-Path $placeholderPath))) {
    Set-Content -Path $placeholderPath -Value $placeholderContent -Encoding UTF8
    Write-Host ("Created placeholder: {0}" -f (ConvertTo-RelativePath $placeholderPath)) -ForegroundColor Gray
  }

  if ($CleanPlaceholdersOnly) {
    if ($realFiles.Count -gt 0) {
      if (Test-Path $placeholderPath) {
        Remove-Item $placeholderPath -Force
        Write-Host ("Deleted placeholder in folder with real files: {0}" -f (ConvertTo-RelativePath $path)) -ForegroundColor DarkGreen
      }
    }
  }
  else {
    Get-ChildItem -Path $path -File | Where-Object { $_.Name -ne "placeholder.txt" } | ForEach-Object {
      Remove-Item $_.FullName -Force
      Write-Host ("Deleted: {0}" -f (ConvertTo-RelativePath $_.FullName)) -ForegroundColor DarkYellow
    }

  }
}

Write-Host ""
Write-Host ("✔ {0} completed successfully." -f $(if ($CleanPlaceholdersOnly) { "Placeholder cleanup" } else { "Export environment initialization" })) -ForegroundColor Green
