# ==============================================================================
# AI-Assisted Professional Engineering with .NET
# Commit C14 - Improve Repository Bootstrap Scripts
# ==============================================================================

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "============================================================"
Write-Host " Commit C14 - Improve Repository Bootstrap Scripts"
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
# Stage Bootstrap Toolkit
#------------------------------------------------------------------------------

$path = "eng"

if (-not (Test-Path $path)) {
  throw "Directory not found: $path"
}

git add $path

#------------------------------------------------------------------------------
# Preview
#------------------------------------------------------------------------------

Write-Host ""
Write-Host "Files staged for Commit C14:"
Write-Host ""

git status --short

Write-Host ""
$answer = Read-Host "Create Commit C14? (Y/N)"

if ($answer -notin @("Y", "y")) {
  Write-Host ""
  Write-Host "Commit cancelled."
  exit
}

#------------------------------------------------------------------------------
# Commit
#------------------------------------------------------------------------------

git commit `
  -m "chore(tooling): improve repository bootstrap scripts" `
  -m @"
Improve the repository bootstrap scripts.

This commit refines the internal tooling used during
the repository bootstrap process.

The improvements enhance reliability, consistency,
and maintainability of the Git automation scripts,
ensuring they better reflect the finalized repository
structure established during the initial development
phase.

These changes conclude the repository bootstrap effort
prior to the first public release.
"@

if ($LASTEXITCODE -ne 0) {
  Write-Host ""
  Write-Host "Commit failed."
  exit 1
}

Write-Host ""
Write-Host "Commit C14 completed successfully."
Write-Host ""