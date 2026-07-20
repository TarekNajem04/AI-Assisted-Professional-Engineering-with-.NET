Write-Host ""
Write-Host "============================================================"
Write-Host " README Milestone 01"
Write-Host " Repository Landing Page Redesign"
Write-Host "============================================================"
Write-Host ""

git add README.md

git status

Write-Host ""
$answer = Read-Host "Create Commit? (Y/N)"

if ($answer -eq "Y") {
  git commit `
    -m "docs(readme): redesign repository landing page" `
    -m "Completely redesign the repository README to reflect the project's new engineering identity.

Highlights:
- Rewrite the introduction from scratch
- Position the repository as an engineering project
- Add Quick Links section
- Add Project Architecture diagram
- Improve repository navigation
- Align README with the Engineering Manifesto, Brand Strategy, and Vision 2030."

  if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Commit failed."
    exit 1
  }

  Write-Host ""
  Write-Host "README milestone committed successfully."
}