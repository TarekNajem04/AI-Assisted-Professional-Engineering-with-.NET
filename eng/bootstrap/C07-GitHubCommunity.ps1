# ==============================================================================
# AI-Assisted Professional Engineering with .NET
# Commit C07 - GitHub Community Health
# ==============================================================================

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "============================================================"
Write-Host " Commit C07 - GitHub Community Health"
Write-Host "============================================================"
Write-Host ""

if (-not (Test-Path ".git")) {
  throw "Current directory is not a Git repository."
}

git restore --staged .

$paths = @(
  ".github/ISSUE_TEMPLATE",
  ".github/pull_request_template.md"
)

foreach ($path in $paths) {
  if (-not (Test-Path $path)) {
    throw "Missing required path: $path"
  }

  git add $path
}

Write-Host ""
Write-Host "Files staged for Commit C07:"
Write-Host ""

git status --short

Write-Host ""
$answer = Read-Host "Create Commit C07? (Y/N)"

if ($answer -notin @("Y", "y")) {
  Write-Host ""
  Write-Host "Commit cancelled."
  exit
}

git commit `
  -m "docs(github): add community health files" `
  -m @"
Add the GitHub community health files.

This commit introduces issue templates and pull request
guidelines to improve communication, feedback quality,
and repository maintenance.

These templates help establish consistent interactions
between readers, contributors, and project maintainers.
"@

if ($LASTEXITCODE -ne 0) {
  Write-Host ""
  Write-Host "Commit failed."
  exit 1
}

Write-Host ""
Write-Host "Commit C07 completed successfully."
Write-Host ""