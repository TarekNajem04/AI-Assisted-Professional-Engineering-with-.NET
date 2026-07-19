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
<#
.SYNOPSIS
    Temporary workspace management.

.NOTES
    Professional Book Export Pipeline
#>

Set-StrictMode -Version Latest

function New-Workspace {

    $Folder =
        Join-Path `
            ([System.IO.Path]::GetTempPath()) `
            ("book-export-" + [guid]::NewGuid())

    New-Item `
        -ItemType Directory `
        -Force `
        -Path $Folder | Out-Null

    return $Folder

}

function Remove-Workspace {

    param(
        [string]$Workspace
    )

    if(Test-Path $Workspace)
    {
        Remove-Item `
            $Workspace `
            -Recurse `
            -Force
    }

}




