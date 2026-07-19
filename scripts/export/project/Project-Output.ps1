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

function Get-OutputDirectory {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$ProjectRoot,
        [Parameter(Mandatory)]
        [string]$Type,
        [Parameter(Mandatory)]
        [string]$Language,
        [Parameter(Mandatory)]
        [string]$Category
    )
    return Join-Path $ProjectRoot ("exports/{0}/{1}/{2}" -f $Type, $Language, $Category)
}

function Get-OutputFilename {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$BaseName,
        [Parameter(Mandatory)]
        [string]$Type
    )
    return "$BaseName.$Type"
}

function Get-OutputFile {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$ProjectRoot,
        [Parameter(Mandatory)]
        [string]$BaseName,
        [Parameter(Mandatory)]
        [string]$Type,
        [Parameter(Mandatory)]
        [string]$Language,
        [Parameter(Mandatory)]
        [string]$Category
    )
    $directory = Get-OutputDirectory -ProjectRoot $ProjectRoot -Type $Type -Language $Language -Category $Category
    $filename = Get-OutputFilename -BaseName $BaseName -Type $Type
    return Join-Path $directory $filename
}

