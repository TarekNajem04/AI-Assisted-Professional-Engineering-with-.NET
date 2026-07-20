# ==============================================================================
# AI-Assisted Professional Engineering with .NET
# Commit C15 - Remove Placeholder Workflows
# ==============================================================================

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "============================================================"
Write-Host " Commit C15 - Remove Placeholder Workflows"
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
# Stage Workflow Removals
#------------------------------------------------------------------------------

$paths = @(
  ".github/workflows/docs-build.yml",
  ".github/workflows/export-manuscript.yml"
)

foreach ($path in $paths) {
  git rm --quiet --ignore-unmatch $path
}

#------------------------------------------------------------------------------
# Preview
#------------------------------------------------------------------------------

Write-Host ""
Write-Host "Files staged for Commit C15:"
Write-Host ""

git status --short

Write-Host ""
$answer = Read-Host "Create Commit C15? (Y/N)"

if ($answer -notin @("Y", "y")) {
  Write-Host ""
  Write-Host "Commit cancelled."

  git restore --staged .
  exit
}

#------------------------------------------------------------------------------
# Commit
#------------------------------------------------------------------------------

git commit `
  -m "fix(ci): remove placeholder workflows" `
  -m @"
Remove temporary placeholder GitHub Actions workflows.

The placeholder workflow files caused unintended
workflow executions and have been removed until
fully implemented automation becomes available.

The project currently relies on the local export
toolchain while the CI/CD workflows are being
designed and validated.

Fully functional GitHub Actions workflows will be
introduced in a future release.
"@

if ($LASTEXITCODE -ne 0) {
  Write-Host ""
  Write-Host "Commit failed."
  exit 1
}

Write-Host ""
Write-Host "Commit C15 completed successfully."
Write-Host ""