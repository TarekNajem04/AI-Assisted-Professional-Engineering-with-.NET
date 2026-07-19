# ==============================================================================
# AI-Assisted Professional Engineering with .NET
# Commit C04 - Publication Strategy
# ==============================================================================

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "============================================================"
Write-Host " Commit C04 - Publication Strategy"
Write-Host "============================================================"
Write-Host ""

if (-not (Test-Path ".git")) {
  throw "Current directory is not a Git repository."
}

git restore --staged .

$directories = @(
  "docs/publishing",
  "docs/versioning"
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
Write-Host "Files staged for Commit C04:"
Write-Host ""

git status --short

Write-Host ""
$answer = Read-Host "Create Commit C04? (Y/N)"

if ($answer -notin @("Y", "y")) {
  Write-Host ""
  Write-Host "Commit cancelled."
  exit
}

git commit `
  -m "docs(publication): define publication and release strategy" `
  -m @"
Define the project's publication strategy.

This commit introduces the documentation governing
incremental publication, release management,
versioning, and long-term publishing roadmap.

These documents establish the editorial workflow
used to publish the manuscript section by section
while maintaining consistency across future releases.
"@

if ($LASTEXITCODE -ne 0) {
  Write-Host ""
  Write-Host "Commit failed."
  exit 1
}

Write-Host ""
Write-Host "Commit C04 completed successfully."
Write-Host ""