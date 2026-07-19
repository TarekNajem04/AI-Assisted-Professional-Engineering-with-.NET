<#
    SPDX-License-Identifier: MIT
    Copyright (c) 2026 Tarek Najem

    This file is part of the AI-Assisted Professional Engineering with .NET
    book project (https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET).
    It is subject to the terms and conditions of the MIT License as published
    in the LICENSE file at the root of this repository.

    This header must not be removed or modified without preserving the
    LICENSE file reference and copyright notice.
#>
Set-StrictMode -Version Latest

function Invoke-Docx {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory)]
    $Context
  )

  Write-Log "[Stage] DOCX" Section

  $arguments = Get-PandocDocxArguments -Context $Context
  Invoke-Pandoc -Context $Context -Arguments $arguments

  $hlScript = Join-Path $Context.RepositoryRoot "scripts/export/docx-apply-hl-colors.py"
  if (Test-Path $hlScript) {
    Write-Log "  [Post-process] Normalising highlight colours…" Muted
    & python @($hlScript, $Context.DocxOutputFile, "--profile", $Context.StyleProfileName) 2>&1 | ForEach-Object { Write-Log "    $_" Muted }
  }

  $fitScript = Join-Path $Context.RepositoryRoot "scripts/export/docx-fit-images.py"
  if (Test-Path $fitScript) {
    Write-Log "  [Post-process] Fitting images to page..." Muted
    & python $fitScript $Context.DocxOutputFile --caption-reserve-cm 1.4 2>&1 | ForEach-Object { Write-Log "    $_" Muted }
  }

  $configPath = Join-Path $Context.RepositoryRoot "scripts/export/export-config.json"

  $marginScript = Join-Path $Context.RepositoryRoot "scripts/export/docx-set-margins.py"
  if (Test-Path $marginScript) {
    $marginArgs = @(
      $marginScript,
      $Context.DocxOutputFile,
      "--config", $configPath,
      "--repo-root", $Context.RepositoryRoot
    )
    & python @marginArgs | ForEach-Object { Write-Host $_ }
  }

  if ($Context.Config.Docx.CoverPages) {
    $pythonScript = Join-Path $Context.RepositoryRoot "scripts/export/docx-add-covers.py"
    $pythonArgs = @(
      $pythonScript,
      $Context.DocxOutputFile
    )
    if ($Context.Cover.Front) { $pythonArgs += "--front"; $pythonArgs += $Context.Cover.Front }
    if ($Context.Cover.Back)  { $pythonArgs += "--back";  $pythonArgs += $Context.Cover.Back }
    $pythonArgs += "--repo-root"; $pythonArgs += $Context.RepositoryRoot
    $pythonArgs += "--config"; $pythonArgs += $configPath

    & python @pythonArgs | ForEach-Object { Write-Host $_ }
    if ($LASTEXITCODE -ne 0) {
        throw "Python DOCX processor failed."
    }
  }

  if (Test-Path $Context.DocxOutputFile) {
    Write-Log "  [OK] $(ConvertTo-RelativePath $Context.DocxOutputFile)" Success
  }
  else {
    throw "DOCX output not created: $($Context.DocxOutputFile)"
  }
}

