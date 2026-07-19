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

function Invoke-Pandoc {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory)]
    $Context,
    [Parameter(Mandatory)]
    [string[]]
    $Arguments
  )

  Write-Log
  Write-Log "[Stage] Pandoc" Section

  & $Context.Tools.Pandoc @Arguments

  if ($LASTEXITCODE -ne 0) {
    throw "Pandoc exited with code $LASTEXITCODE."
  }
}

