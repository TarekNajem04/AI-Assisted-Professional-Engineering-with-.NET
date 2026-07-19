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
<#
.SYNOPSIS
    Discovers external tools used by the export pipeline.
#>

Set-StrictMode -Version Latest

function Get-ProjectTools {

  [CmdletBinding()]
  param()

  return [pscustomobject]@{

    Pandoc =
    (Get-Command pandoc -ErrorAction Stop).Source

    MermaidCli =
    (Get-Command mmdc -ErrorAction SilentlyContinue)?.Source

    ImageMagick =
    (Get-Command magick -ErrorAction SilentlyContinue)?.Source

  }

}



