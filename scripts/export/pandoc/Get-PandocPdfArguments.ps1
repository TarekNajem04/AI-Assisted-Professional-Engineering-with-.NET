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

function Get-PandocPdfArguments {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory)]
    $Context
  )

  $hl = $Context.Config.Pdf.SyntaxHighlighting
  $arguments = Get-PandocCommonArguments -Context $Context -SyntaxHighlighting $hl

  $arguments += "--to=pdf"
  $arguments += "--pdf-engine=$($Context.PdfEngine)"

  $fontSize = $Context.Config.Pdf.FontSize
  if (-not $fontSize) { $fontSize = '12pt' }
  $arguments += "-V"
  $arguments += "fontsize=$fontSize"

  $arguments += "--output"
  $arguments += $Context.PdfOutputFile

  return $arguments
}

