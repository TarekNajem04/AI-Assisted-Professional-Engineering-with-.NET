# ==============================================================================
# AI-Assisted Professional Engineering with .NET
# Commit C10 - Export Pipeline
# ==============================================================================

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "============================================================"
Write-Host " Commit C10 - Export Pipeline"
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
# Stage Export Pipeline
#------------------------------------------------------------------------------

$path = "scripts/export"

if (-not (Test-Path $path)) {
  throw "Directory not found: $path"
}

git add $path

#------------------------------------------------------------------------------
# Preview
#------------------------------------------------------------------------------

Write-Host ""
Write-Host "Files staged for Commit C10:"
Write-Host ""

git status --short

Write-Host ""
$answer = Read-Host "Create Commit C10? (Y/N)"

if ($answer -notin @("Y", "y")) {
  Write-Host ""
  Write-Host "Commit cancelled."
  exit
}

#------------------------------------------------------------------------------
# Commit
#------------------------------------------------------------------------------

git commit `
  -m "build(export): enhance manuscript export pipeline" `
  -m @"
Enhance the manuscript export pipeline.

This commit improves the local export infrastructure
used to transform the manuscript into publication-ready
formats.

The changes refine the export workflow, preprocessing
pipeline, style generation, document validation, and
supporting utilities to improve reliability,
maintainability, and output quality.

These enhancements establish a solid foundation for
future automated publishing and release workflows.
"@

if ($LASTEXITCODE -ne 0) {
  Write-Host ""
  Write-Host "Commit failed."
  exit 1
}

Write-Host ""
Write-Host "Commit C10 completed successfully."
Write-Host ""