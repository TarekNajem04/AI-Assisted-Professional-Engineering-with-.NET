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

function Invoke-Preprocessor {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory)]
    $Context
  )

  Write-Log
  Write-Log "[Stage] Preprocessor" Section

  $preprocessorsDir = Join-Path $PSScriptRoot "../preprocessors"

  . (Join-Path $preprocessorsDir "MarkdownPreprocessor.ps1")
  Invoke-MarkdownPreprocessor -Context $Context

  . (Join-Path $preprocessorsDir "MermaidPreprocessor.ps1")
  Invoke-MermaidPreprocessor -Context $Context

  . (Join-Path $preprocessorsDir "CSharpPreprocessor.ps1")
  Invoke-CSharpPreprocessor -Context $Context

  . (Join-Path $preprocessorsDir "Invoke-CoverBuilder.ps1")
  Invoke-CoverBuilder -Context $Context

  Write-Log "  [OK] Preprocessor completed."
}

