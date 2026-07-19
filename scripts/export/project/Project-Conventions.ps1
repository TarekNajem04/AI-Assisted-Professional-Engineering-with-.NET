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
    Resolves ProfBook project conventions.

.DESCRIPTION

    This module knows only the ProfBook project layout.

    It does NOT know:

        - Pandoc
        - DOCX
        - PDF
        - LuaLaTeX

    It only answers questions such as:

        • Which language is this file?
        • What kind of document is it?
        • How are project folders organized?

.NOTES

    ProfBook vNext
#>

Set-StrictMode -Version Latest

function Normalize-Path {

    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    return $Path.Replace('\', '/')

}

function Get-DocumentLanguage {

    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$SourceFile
    )

    if ($SourceFile.EndsWith('.ar.md')) { return 'ar' }

    if ($SourceFile.EndsWith('.en.md')) { return 'en' }

    throw "Cannot determine document language."

}

function Get-DocumentKind {

    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string[]]$PathSegments
    )

    $kindMap = @{

        'assembled'  = 'chapter'

        'sections'   = 'section'

        'manifesto'  = 'manifesto'

        'tests'      = 'tests'

    }

    foreach ($segment in $PathSegments) {

        if ($kindMap.ContainsKey($segment)) {

            return $kindMap[$segment]

        }

    }

    throw "Cannot determine document kind."

}

function Get-DocumentInfo {

    [CmdletBinding()]
    param(

        [Parameter(Mandatory)]
        [string]$SourceFile,

        [Parameter(Mandatory)]
        [string]$ProjectRoot

    )

    $normalizedPath =
    Normalize-Path -Path $SourceFile

    $segments =
    $normalizedPath.Split('/')

    $language =
    Get-DocumentLanguage `
      -SourceFile $SourceFile

    $kind =
    Get-DocumentKind `
      -PathSegments $segments

    $kindToCategory = @{

        'chapter'   = 'chapters'

        'section'   = 'sections'

        'manifesto' = 'manifesto'

        'tests'     = 'tests'

    }

    $category = $kindToCategory[$kind]

    $normalizedRoot =
    Normalize-Path -Path $ProjectRoot

    $rootPrefix = "$normalizedRoot/"

    $relativePath = $normalizedPath

    if ($normalizedPath.StartsWith($rootPrefix, [StringComparison]::OrdinalIgnoreCase)) {

        $relativePath =
        $normalizedPath.Substring($rootPrefix.Length)

    }

    return [pscustomobject]@{

      SourceFile   = $SourceFile

      FileName     =
      [IO.Path]::GetFileName($normalizedPath)

      BaseName     =
      [IO.Path]::GetFileNameWithoutExtension($normalizedPath)

      RelativePath = $relativePath

      Language     = $language

      Kind         = $kind

      Category     = $category

    }

}




