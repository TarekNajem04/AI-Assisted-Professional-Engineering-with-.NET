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

$script:LogColors = $null

function Initialize-LogColors {
  param([object]$Config)
  if ($Config -and $Config.Log -and $Config.Log.Colors) {
    $script:LogColors = $Config.Log.Colors
  }
}

function Write-Log {
  [CmdletBinding()]
  param(
    [Parameter(Position = 0)]
    [string]$Message = '',
    [Parameter(Position = 1)]
    [string]$ColorKey = 'Info',
    [switch]$NoNewline
  )
  if ([string]::IsNullOrEmpty($Message)) {
    Write-Host ''
    return
  }
  $color = $null
  if ($script:LogColors -and $script:LogColors.$ColorKey) {
    $color = $script:LogColors.$ColorKey
  }
  if ($color) {
    if ($NoNewline) { Write-Host $Message -ForegroundColor $color -NoNewline }
    else { Write-Host $Message -ForegroundColor $color }
  } else {
    if ($NoNewline) { Write-Host $Message -NoNewline }
    else { Write-Host $Message }
  }
}

