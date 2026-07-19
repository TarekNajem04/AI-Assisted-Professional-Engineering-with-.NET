# ==============================================================================
# AI-Assisted Professional Engineering with .NET
# Commit C11 - Repository Maintenance Toolkit
# ==============================================================================

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "============================================================"
Write-Host " Commit C11 - Repository Maintenance Toolkit"
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
# Stage Repository Toolkit
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
Write-Host "Files staged for Commit C11:"
Write-Host ""

git status --short

Write-Host ""
$answer = Read-Host "Create Commit C11? (Y/N)"

if ($answer -notin @("Y", "y")) {
  Write-Host ""
  Write-Host "Commit cancelled."
  exit
}

#------------------------------------------------------------------------------
# Commit
#------------------------------------------------------------------------------

git commit `
  -m "chore(tooling): add repository maintenance toolkit" `
  -m @"
Add the repository maintenance toolkit.

This commit introduces the internal tooling used to
maintain, automate, and manage the repository.

The toolkit includes Git automation, repository
utilities, and engineering scripts intended for
project maintainers.

These tools are not part of the published manuscript,
but support the long-term maintenance and evolution
of the project.
"@

if ($LASTEXITCODE -ne 0) {
  Write-Host ""
  Write-Host "Commit failed."
  exit 1
}

Write-Host ""
Write-Host "Commit C11 completed successfully."
Write-Host ""