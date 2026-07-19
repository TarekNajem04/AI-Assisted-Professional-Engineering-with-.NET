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

function Invoke-CSharpPreprocessor {
  param([object]$Context)

  $mdPath = $Context.Working.Markdown
  if (-not (Test-Path $mdPath)) { return }

  $content = Get-Content -LiteralPath $mdPath -Raw -Encoding UTF8

  $pattern = '(?ms)```(?:csharp|cs)(?:\s+[^\r\n]*)?\r?\n(.*?)\r?\n```'
  $newline  = "`r`n"
  $opening  = '```csharp {.monospace}'
  $closing  = '```'
  $code     = '$1'

  $newContent = [regex]::Replace($content, $pattern, $opening + $newline + $code + $newline + $closing)

  Set-Content -LiteralPath $mdPath -Value $newContent -Encoding UTF8

  $matches = [regex]::Matches($newContent, '```(?:csharp|cs)(.*?)```', 'Singleline')
  $Context.CSharp.BlockCount = $matches.Count
}

