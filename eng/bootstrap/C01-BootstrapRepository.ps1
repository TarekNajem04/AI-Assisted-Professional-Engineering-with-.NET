# ==============================================================================
# AI-Assisted Professional Engineering with .NET
# Commit C01 - Bootstrap Repository Foundation
# ==============================================================================

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "============================================================"
Write-Host " Commit C01 - Bootstrap Repository Foundation"
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
# Files
#------------------------------------------------------------------------------

$files = @(
  ".gitattributes",
  ".gitignore",
  "LICENSE",
  "README.md"
)

foreach ($file in $files) {
  if (-not (Test-Path $file)) {
    throw "Missing required file: $file"
  }

  git add $file
}

#------------------------------------------------------------------------------
# Preview
#------------------------------------------------------------------------------

Write-Host ""
Write-Host "Files staged for Commit C01:"
Write-Host ""

git status --short

Write-Host ""
$answer = Read-Host "Create Commit C01? (Y/N)"

if ($answer -notin @("Y", "y")) {
  Write-Host ""
  Write-Host "Commit cancelled."
  exit
}

#------------------------------------------------------------------------------
# Commit
#------------------------------------------------------------------------------

git commit `
  -m "chore(repository): bootstrap project foundation" `
  -m @"
Establish the initial repository foundation.

This commit creates the project's initial identity,
repository configuration, licensing, and public entry
point.

The repository is now prepared for incremental
development and publication.
"@

if ($LASTEXITCODE -ne 0) {
  Write-Host ""
  Write-Host "Commit failed."
  exit 1
}

Write-Host ""
Write-Host "Commit C01 completed successfully."
Write-Host ""