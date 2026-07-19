# ==============================================================================
# AI-Assisted Professional Engineering with .NET
# Commit C09 - Publication Assets
# ==============================================================================

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "============================================================"
Write-Host " Commit C09 - Publication Assets"
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
# Validate Assets
#------------------------------------------------------------------------------

$assetsDirectory = "docs/assets"

if (-not (Test-Path $assetsDirectory)) {
  throw "Directory not found: $assetsDirectory"
}

git add $assetsDirectory

#------------------------------------------------------------------------------
# Preview
#------------------------------------------------------------------------------

Write-Host ""
Write-Host "Files staged for Commit C09:"
Write-Host ""

git status --short

Write-Host ""
$answer = Read-Host "Create Commit C09? (Y/N)"

if ($answer -notin @("Y", "y")) {
  Write-Host ""
  Write-Host "Commit cancelled."
  exit
}

#------------------------------------------------------------------------------
# Commit
#------------------------------------------------------------------------------

git commit `
  -m "docs(assets): add publication assets and document templates" `
  -m @"
Add the publication assets and document templates.

This commit introduces the visual assets and styling
resources used to produce consistent publication
outputs across PDF, DOCX, and future distribution
formats.

These assets define the project's visual identity
and document presentation.
"@

if ($LASTEXITCODE -ne 0) {
  Write-Host ""
  Write-Host "Commit failed."
  exit 1
}

Write-Host ""
Write-Host "Commit C09 completed successfully."
Write-Host ""