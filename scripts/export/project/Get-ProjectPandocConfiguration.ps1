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

function Get-ProjectPandocConfiguration {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory)]
    $Context
  )

  $configuration = [ordered]@{
    ResourcePaths = @()
    LuaFilters    = @()
    Headers       = @()
    Variables     = @()
    Metadata      = @()
  }

  $configuration.ResourcePaths += $Context.RepositoryRoot
  $configuration.ResourcePaths += (Join-Path $Context.RepositoryRoot "docs")
  $configuration.ResourcePaths += (Join-Path $Context.RepositoryRoot "docs/assets")
  $configuration.ResourcePaths += (Join-Path $Context.RepositoryRoot "docs/assets/images")
  $configuration.ResourcePaths += (Join-Path $Context.RepositoryRoot "docs/assets/diagrams")

  if ($Context.Language -eq "ar") {
    $configuration.Metadata += "dir=rtl"
  }

  $title = $Context.Config.Pdf.Title
  if ($title) {
    $configuration.Metadata += "title=$title"
  }

  $configuration.LuaFilters += $Context.Paths.LuaFilterCodeblockBidi

  $themeFile = $Context.Paths.ThemeFile
  if ($Context.Working.ThemeFile -and (Test-Path $Context.Working.ThemeFile)) {
    $themeFile = $Context.Working.ThemeFile
  }
  $configuration.Headers += $themeFile

  if ($Context.Language -eq "ar") {
    $configuration.Headers += $Context.Paths.ArabicFile
  }

  $configuration.LuaFilters += $Context.Paths.LuaFilterEscapeBackslash

  if ($Context.Cover.HeaderFile -and (Test-Path $Context.Cover.HeaderFile)) {
    $configuration.Headers = @($Context.Cover.HeaderFile) + $configuration.Headers
  }

  return [pscustomobject]$configuration
}

