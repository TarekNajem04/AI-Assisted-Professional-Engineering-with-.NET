# ==============================================================================
# AI-Assisted Professional Engineering with .NET
# Commit C13 - Prepare Initial Release
# ==============================================================================

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "============================================================"
Write-Host " Commit C13 - Prepare Initial Release"
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
# Stage Release Preparation Files
#------------------------------------------------------------------------------

$paths = @(
  "exports",
  ".github/CODEOWNERS",
  ".gitignore",
  "LICENSE",
  "docs/assets/front-cover-template.html"
)

foreach ($path in $paths) {
  if (-not (Test-Path $path)) {
    throw "Missing required path: $path"
  }

  git add $path
}

#------------------------------------------------------------------------------
# Preview
#------------------------------------------------------------------------------

Write-Host ""
Write-Host "Files staged for Commit C13:"
Write-Host ""

git status --short

Write-Host ""
$answer = Read-Host "Create Commit C13? (Y/N)"

if ($answer -notin @("Y", "y")) {
  Write-Host ""
  Write-Host "Commit cancelled."
  exit
}

#------------------------------------------------------------------------------
# Commit
#------------------------------------------------------------------------------

git commit `
  -m "build(release): prepare repository for initial release" `
  -m @"
Prepare the repository for the initial public release.

This commit publishes the first generated manuscript
artifacts, finalizes repository ownership metadata,
updates repository configuration, and refines the
publication assets.

These changes complete the repository bootstrap phase
and prepare the project for its first public release.
"@

if ($LASTEXITCODE -ne 0) {
  Write-Host ""
  Write-Host "Commit failed."
  exit 1
}

Write-Host ""
Write-Host "Commit C13 completed successfully."
Write-Host ""