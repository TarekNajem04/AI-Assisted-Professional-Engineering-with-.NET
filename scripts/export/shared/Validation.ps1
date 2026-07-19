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

function Test-ExportContext {
    param($Context)

    if (!(Test-Path $Context.SourceFile)) {
        throw "Source file not found:`n$($Context.SourceFile)"
    }

    $Files = @(
        $Context.Paths.ThemeFile,
        $Context.Paths.ArabicFile
    )

    foreach ($File in $Files) {
        if (!(Test-Path $File)) {
            throw "Missing required file:`n$File"
        }
    }

    $outputDirs = @(
        (Split-Path $Context.PdfOutputFile -Parent),
        (Split-Path $Context.DocxOutputFile -Parent)
    )

    foreach ($dir in $outputDirs) {
        if (!(Test-Path $dir)) {
            New-Item -ItemType Directory -Force -Path $dir | Out-Null
        }
    }
}

