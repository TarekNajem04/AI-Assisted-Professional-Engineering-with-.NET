# ==============================================================================
# AI-Assisted Professional Engineering with .NET
# Commit C03 - Governance
# ==============================================================================

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "============================================================"
Write-Host " Commit C03 - Governance"
Write-Host "============================================================"
Write-Host ""

if (-not (Test-Path ".git")) {
  throw "Current directory is not a Git repository."
}

git restore --staged .

$paths = @(
  "CODE_OF_CONDUCT.md",
  "COMMUNITY_GUIDELINES.md",
  "CONTRIBUTING.md",
  "SECURITY.md",
  "SUPPORT.md"
)

foreach ($path in $paths) {
  if (-not (Test-Path $path)) {
    throw "Missing required file: $path"
  }

  git add $path
}

$optionalDirectories = @(
  "docs/governance",
  "docs/project-status",
  "docs/roadmap"
)

foreach ($directory in $optionalDirectories) {
  if (Test-Path $directory) {
    git add $directory
  }
}

Write-Host ""
Write-Host "Files staged for Commit C03:"
Write-Host ""

git status --short

Write-Host ""
$answer = Read-Host "Create Commit C03? (Y/N)"

if ($answer -notin @("Y", "y")) {
  Write-Host ""
  Write-Host "Commit cancelled."
  exit
}

git commit `
  -m "docs(governance): establish community and governance guidelines" `
  -m @"
Establish the project's governance foundation.

This commit introduces the community standards,
contribution policy, code of conduct, security policy,
support guidelines, and governance documentation.

These documents define how the project is maintained,
how the community can participate, and how the
repository will evolve during its early publication
phases.
"@

if ($LASTEXITCODE -ne 0) {
  Write-Host ""
  Write-Host "Commit failed."
  exit 1
}

Write-Host ""
Write-Host "Commit C03 completed successfully."
Write-Host ""