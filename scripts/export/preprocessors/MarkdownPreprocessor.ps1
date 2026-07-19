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

function Invoke-MarkdownPreprocessor {
  param([object]$Context)

  if (-not (Test-Path $Context.SourceFile)) {
    throw "Source file not found: $($Context.SourceFile)"
  }

  $content = Get-Content -LiteralPath $Context.SourceFile -Raw -Encoding UTF8

  $preprocessedPath = Join-Path $Context.Working.Directory "preprocessed.md"
  Set-Content -LiteralPath $preprocessedPath -Value $content -Encoding UTF8

  $Context.Working.Markdown = $preprocessedPath
}

