<#
    SPDX-License-Identifier: MIT
    Copyright (c) 2025 Tarek Najem

    This file is part of the AI-Assisted Professional Engineering with .NET
    book project (https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET).
    It is subject to the terms and conditions of the MIT License as published
    in the LICENSE file at the root of this repository.

    This header must not be removed or modified without preserving the
    LICENSE file reference and copyright notice.
#>
Set-StrictMode -Version Latest

function Invoke-DocxStyleValidation {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory)]
    $Context
  )

  Write-Log "[Validation] DOCX Style Validation..." Section

  if (-not (Test-Path $Context.TemplateFilePath)) {
    Write-Log "  [SKIP] Template not found: $(ConvertTo-RelativePath $Context.TemplateFilePath)" Warning
    return $false
  }

  # Try profile-specific validator first, fall back to shared one
  $profileValidator = Join-Path (Join-Path $Context.RepositoryRoot "scripts/export/styles") "$($Context.StyleProfileName)/docx-validate-styles.py"
  $sharedValidator = Join-Path $Context.RepositoryRoot "scripts/export/docx-validate-styles.py"
  $pythonScript = if (Test-Path $profileValidator) { $profileValidator } else { $sharedValidator }

  if (-not (Test-Path $pythonScript)) {
    Write-Log "  [SKIP] Validation script not found: $(ConvertTo-RelativePath $pythonScript)" Warning
    return $false
  }

  $profileArg = "--profile", "$($Context.StyleProfileName)"
  $projectRootArg = "--project-root", "$($Context.RepositoryRoot)"
  $output = & python $pythonScript --reference-doc "$($Context.TemplateFilePath)" $profileArg $projectRootArg 2>&1 | ForEach-Object { "$_" }

  $passed = $true
  foreach ($line in $output) {
    if ($line -match '\bFAIL\b|\bERROR\b|\bMISSING\b') { $passed = $false }
    Write-Log "    $line" Muted
  }

  if (-not $passed) {
    Write-Log "  [FAIL] DOCX style validation failed." Error
  } else {
    Write-Log "  [OK] All DOCX styles validated successfully." Success
  }

  return $passed
}

