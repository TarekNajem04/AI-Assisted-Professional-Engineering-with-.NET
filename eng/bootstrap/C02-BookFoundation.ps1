# ==============================================================================
# AI-Assisted Professional Engineering with .NET
# Commit C02 - Book Foundation
# ==============================================================================

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "============================================================"
Write-Host " Commit C02 - Book Foundation"
Write-Host "============================================================"
Write-Host ""

if (-not (Test-Path ".git")) {
  throw "Current directory is not a Git repository."
}

git restore --staged .

$paths = @(
  "TOC.en.md",
  "TOC.ar.md",
  "book"
)

foreach ($path in $paths) {
  if (-not (Test-Path $path)) {
    throw "Missing required path: $path"
  }

  git add $path
}

Write-Host ""
Write-Host "Files staged for Commit C02:"
Write-Host ""

git status --short

Write-Host ""
$answer = Read-Host "Create Commit C02? (Y/N)"

if ($answer -notin @("Y", "y")) {
  Write-Host ""
  Write-Host "Commit cancelled."
  exit
}

git commit `
  -m "docs(book): introduce bilingual manuscript foundation" `
  -m @"
Introduce the initial bilingual manuscript structure.

This commit adds the project's table of contents and
the bilingual introduction, establishing the initial
public manuscript layout for incremental publication.

Future chapters and sections will be published
progressively following the project's publication model.
"@

if ($LASTEXITCODE -ne 0) {
  Write-Host ""
  Write-Host "Commit failed."
  exit 1
}

Write-Host ""
Write-Host "Commit C02 completed successfully."
Write-Host ""