# ==============================================================================
# AI-Assisted Professional Engineering with .NET
# Commit C12 - Repository Documentation
# ==============================================================================

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "============================================================"
Write-Host " Commit C12 - Repository Documentation"
Write-Host "============================================================"
Write-Host ""

#------------------------------------------------------------------------------
# Validate Git Repository
#------------------------------------------------------------------------------

if (-not (Test-Path ".git")) {
  throw "Current directory is not a Git repository."
}

#------------------------------------------------------------------------------
# Clear Stage
#------------------------------------------------------------------------------

git restore --staged .

#------------------------------------------------------------------------------
# Stage Repository Documentation
#------------------------------------------------------------------------------

$paths = @(
  "docs/AR",
  "docs/EN"
)

foreach ($path in $paths) {
  if (Test-Path $path) {
    git add $path
  }
  else {
    Write-Warning "Directory not found: $path"
  }
}

#------------------------------------------------------------------------------
# Preview
#------------------------------------------------------------------------------

Write-Host ""
Write-Host "Files staged for Commit C12:"
Write-Host ""

git status --short

Write-Host ""
$answer = Read-Host "Create Commit C12? (Y/N)"

if ($answer -notin @("Y", "y")) {
  Write-Host ""
  Write-Host "Commit cancelled."
  exit
}

#------------------------------------------------------------------------------
# Commit
#------------------------------------------------------------------------------

git commit `
  -m "docs(repository): add repository documentation" `
  -m @"
Add repository documentation.

This commit introduces supplementary documentation that
describes the repository organization, engineering
workflow, publication process, and project resources.

These documents complement the manuscript and help
readers, contributors, and maintainers understand how
the repository is structured and managed.
"@

if ($LASTEXITCODE -ne 0) {
  Write-Host ""
  Write-Host "Commit failed."
  exit 1
}

Write-Host ""
Write-Host "Commit C12 completed successfully."
Write-Host ""