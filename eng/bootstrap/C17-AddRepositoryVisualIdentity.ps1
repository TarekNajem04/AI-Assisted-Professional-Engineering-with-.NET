Write-Host ""
Write-Host "============================================================"
Write-Host " Branding Milestone 01"
Write-Host " Repository Visual Identity"
Write-Host "============================================================"
Write-Host ""

git add "AI-assisted professional engineering blueprint.png"
git add "eng\bootstrap\C17-AddRepositoryVisualIdentity.ps1"

git status

Write-Host ""
$answer = Read-Host "Create Commit? (Y/N)"

if ($answer -eq "Y") {
  git commit `
    -m "docs(branding): add repository visual identity" `
    -m "Introduce the initial visual identity for the repository.

Highlights:
- Add the official engineering blueprint artwork
- Establish the project's visual direction
- Prepare assets for GitHub Social Preview
- Provide a reusable branding asset for future releases."

  if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Commit failed."
    exit 1
  }

  Write-Host ""
  Write-Host "Branding milestone committed successfully."
}