# ==============================================================================
# AI-Assisted Professional Engineering with .NET
# Commit C08 - GitHub Workflows
# ==============================================================================

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "============================================================"
Write-Host " Commit C08 - GitHub Workflows"
Write-Host "============================================================"
Write-Host ""

#------------------------------------------------------------------------------
# Validate Git Repository
#------------------------------------------------------------------------------

if (-not (Test-Path ".git")) {
  throw "Current directory is not a Git repository."
}

#------------------------------------------------------------------------------
# Clean Stage
#------------------------------------------------------------------------------

git restore --staged .

#------------------------------------------------------------------------------
# Validate Workflows
#------------------------------------------------------------------------------

$workflowDirectory = ".github/workflows"

if (-not (Test-Path $workflowDirectory)) {
  throw "Directory not found: $workflowDirectory"
}

git add $workflowDirectory

#------------------------------------------------------------------------------
# Preview
#------------------------------------------------------------------------------

Write-Host ""
Write-Host "Files staged for Commit C08:"
Write-Host ""

git status --short

Write-Host ""
$answer = Read-Host "Create Commit C08? (Y/N)"

if ($answer -notin @("Y", "y")) {
  Write-Host ""
  Write-Host "Commit cancelled."
  exit
}

#------------------------------------------------------------------------------
# Commit
#------------------------------------------------------------------------------

git commit `
  -m "ci(workflows): add automated manuscript build workflows" `
  -m @"
Introduce automated GitHub workflows for manuscript validation
and publication.

This commit adds the continuous integration workflows
used to validate documentation changes and automate
the generation of publication artifacts.

The workflows establish the foundation for a reproducible
and reliable publication pipeline.
"@

if ($LASTEXITCODE -ne 0) {
  Write-Host ""
  Write-Host "Commit failed."
  exit 1
}

Write-Host ""
Write-Host "Commit C08 completed successfully."
Write-Host ""