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

function Invoke-Pdf {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory)]
    $Context
  )

  Write-Log "[Stage] PDF" Section

  $pdfInput = $Context.Working.Markdown

  $content = Get-Content -LiteralPath $pdfInput -Raw -Encoding UTF8
  $content = $content -replace ([char]0x2713), 'v'
  $content = $content -replace ([char]0x2500), '-'
  $content = $content -replace ([char]0x2192), '->'
  Set-Content -LiteralPath $pdfInput -Value $content -Encoding UTF8

  $arguments = Get-PandocPdfArguments -Context $Context
  Invoke-Pandoc -Context $Context -Arguments $arguments

  if (Test-Path $Context.PdfOutputFile) {
    Write-Log "  [OK] $(ConvertTo-RelativePath $Context.PdfOutputFile)" Success
  }
  else {
    throw "PDF output not created: $($Context.PdfOutputFile)"
  }
}

