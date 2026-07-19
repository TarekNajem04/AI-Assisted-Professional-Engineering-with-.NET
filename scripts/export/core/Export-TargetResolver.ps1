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

function Resolve-ExportTarget {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory)]
    [string]$SourceFile,
    [string]$RepositoryRoot,
    [string[]]$ValidLanguages,
    [object[]]$SourceLayouts
  )

  if (-not $RepositoryRoot) {
    $RepositoryRoot = (Get-Location).Path
  }

  $full =
  if ([System.IO.Path]::IsPathRooted($SourceFile)) {
    $SourceFile
  } else {
    [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($RepositoryRoot, $SourceFile))
  }

  if (!(Test-Path $full)) {
    throw "Source file not found: $SourceFile"
  }

  $name = [System.IO.Path]::GetFileNameWithoutExtension($full)

  $langPattern = '(?<base>.+)\.(?<lang>' + ($ValidLanguages -join '|') + ')$'
  if ($name -match $langPattern) {
    $language = $Matches.lang
    $basename = $Matches.base
  } else {
    throw "Cannot determine language from file name: $name"
  }

  $relative = $full.Replace($RepositoryRoot, "").TrimStart('\')
  Write-Log $RepositoryRoot
  Write-Log $relative
  Write-Log $full.Replace($RepositoryRoot, "").TrimStart('\')
  $kind = $null
  foreach ($entry in $SourceLayouts) {
    if ($relative -match $entry.Regex) {
      $kind = $entry.Category
      break
    }
  }

  if (-not $kind) {
    throw "Unsupported source layout: $relative"
  }

  [pscustomobject]@{
    SourceFile = $full
    Language   = $language
    Kind       = $kind
    BaseName   = $basename
  }
}

