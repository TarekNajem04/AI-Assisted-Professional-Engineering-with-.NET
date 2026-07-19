# ==============================================================================
# AI-Assisted Professional Engineering with .NET
# Commit C06 - Local Build Infrastructure
# ==============================================================================

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "============================================================"
Write-Host " Commit C06 - Local Build Infrastructure"
Write-Host "============================================================"
Write-Host ""

if (-not (Test-Path ".git")) {
  throw "Current directory is not a Git repository."
}

git restore --staged .

$paths = @(
  "scripts"
)

foreach ($path in $paths) {
  if (-not (Test-Path $path)) {
    throw "Missing required path: $path"
  }

  git add $path
}

if (Test-Path "tests") {
  git add tests
}

Write-Host ""
Write-Host "Files staged for Commit C06:"
Write-Host ""

git status --short

Write-Host ""
$answer = Read-Host "Create Commit C06? (Y/N)"

if ($answer -notin @("Y", "y")) {
  Write-Host ""
  Write-Host "Commit cancelled."
  exit
}

git commit `
  -m "build(tooling): add local manuscript build infrastructure" `
  -m @"
Introduce the local manuscript build infrastructure.

This commit adds the tooling required to build,
preprocess, and export the manuscript into multiple
publication formats.

The infrastructure enables contributors and maintainers
to generate consistent PDF and DOCX outputs directly
from the Markdown sources using the local toolchain.
"@

if ($LASTEXITCODE -ne 0) {
  Write-Host ""
  Write-Host "Commit failed."
  exit 1
}

Write-Host ""
Write-Host "Commit C06 completed successfully."
Write-Host ""