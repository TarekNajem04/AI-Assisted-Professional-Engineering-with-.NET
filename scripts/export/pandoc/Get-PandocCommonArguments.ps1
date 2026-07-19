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

function Get-PandocCommonArguments {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory)]
    $Context,
    [string]$SyntaxHighlighting
  )

  $pandoc = Get-ProjectPandocConfiguration -Context $Context

  $arguments = @()
  $arguments += $Context.Working.Markdown

  if (-not $SyntaxHighlighting) { $SyntaxHighlighting = 'pygments' }
  $arguments += "--syntax-highlighting=$SyntaxHighlighting"
  $arguments += "--wrap=preserve"

  foreach ($path in $pandoc.ResourcePaths) {
    $arguments += "--resource-path"
    $arguments += $path
  }

  foreach ($filter in $pandoc.LuaFilters) {
    $arguments += "--lua-filter"
    $arguments += $filter
  }

  foreach ($header in $pandoc.Headers) {
    $arguments += "-H"
    $arguments += $header
  }

  foreach ($variable in $pandoc.Variables) {
    $arguments += "-V"
    $arguments += $variable
  }

  foreach ($metadata in $pandoc.Metadata) {
    $arguments += "--metadata"
    $arguments += $metadata
  }

  return $arguments
}

