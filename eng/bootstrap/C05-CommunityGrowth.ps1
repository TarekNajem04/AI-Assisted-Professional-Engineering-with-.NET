# ==============================================================================
# AI-Assisted Professional Engineering with .NET
# Commit C05 - Community Growth & Marketing Strategy
# ==============================================================================

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "============================================================"
Write-Host " Commit C05 - Community Growth & Marketing Strategy"
Write-Host "============================================================"
Write-Host ""

if (-not (Test-Path ".git")) {
  throw "Current directory is not a Git repository."
}

git restore --staged .

$directories = @(
  "docs/marketing"
)

foreach ($directory in $directories) {
  if (Test-Path $directory) {
    git add $directory
  }
  else {
    Write-Warning "Directory not found: $directory"
  }
}

Write-Host ""
Write-Host "Files staged for Commit C05:"
Write-Host ""

git status --short

Write-Host ""
$answer = Read-Host "Create Commit C05? (Y/N)"

if ($answer -notin @("Y", "y")) {
  Write-Host ""
  Write-Host "Commit cancelled."
  exit
}

git commit `
  -m "docs(marketing): establish community growth strategy" `
  -m @"
Establish the project's community growth strategy.

This commit introduces the long-term marketing,
audience growth, and communication plans that will
support the incremental publication of the manuscript.

The strategy focuses on building an engaged
professional community while publishing the book
section by section across multiple platforms.
"@

if ($LASTEXITCODE -ne 0) {
  Write-Host ""
  Write-Host "Commit failed."
  exit 1
}

Write-Host ""
Write-Host "Commit C05 completed successfully."
Write-Host ""