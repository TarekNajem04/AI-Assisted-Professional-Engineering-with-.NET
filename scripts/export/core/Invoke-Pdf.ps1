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

function Resolve-PdfEngine {
  param([string]$Engine)

  $cmd = Get-Command $Engine -ErrorAction SilentlyContinue
  if ($cmd) { return $cmd.Source }

  $exe = "$Engine.exe"
  $searchDirs = @(
    "$env:LOCALAPPDATA\Programs\MiKTeX\miktex\bin\x64",
    "$env:LOCALAPPDATA\Programs\MiKTeX\miktex\bin",
    "$env:ProgramFiles\MiKTeX\miktex\bin\x64",
    "$env:ProgramFiles\MiKTeX\miktex\bin"
  )
  $texliveRoot = 'C:\texlive'
  if (Test-Path $texliveRoot) {
    Get-ChildItem -Path $texliveRoot -Directory -ErrorAction SilentlyContinue |
      ForEach-Object { $searchDirs += Join-Path $_.FullName 'bin\windows' }
  }

  foreach ($dir in $searchDirs) {
    if (-not (Test-Path $dir)) { continue }
    $bin = Join-Path $dir $exe
    if (-not (Test-Path $bin)) { continue }

    $env:PATH = "$dir;$env:PATH"

    $userPath = [Environment]::GetEnvironmentVariable('Path', 'User')
    if ($userPath -and ($userPath -split ';') -notcontains $dir) {
      [Environment]::SetEnvironmentVariable('Path', "$userPath;$dir", 'User')
      Write-Log "  [PDF ENGINE] Added '$dir' to your user PATH (takes effect in new terminals)" Info
    }
    Write-Log "  [PDF ENGINE] '$Engine' found at $bin" Info
    return $bin
  }

  return $null
}

function Invoke-Pdf {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory)]
    $Context
  )

  Write-Log "[Stage] PDF" Section

  $enginePath = Resolve-PdfEngine -Engine $Context.PdfEngine
  if (-not $enginePath) {
    throw "PDF engine '$($Context.PdfEngine)' was not found.`n" +
      "  Fix: install MiKTeX (winget install MiKTeX.MiKTeX) or TeX Live (tug.org/texlive),`n" +
      "  restart your terminal so the engine is on PATH, then re-run the export.`n" +
      "  See scripts\export\PREREQUISITES.md for the full list of requirements."
  }

  $pdfInput = $Context.Working.Markdown

  $content = Get-Content -LiteralPath $pdfInput -Raw -Encoding UTF8
  $content = $content -replace ([char]0x2713), 'v'
  $content = $content -replace ([char]0x2500), '-'
  $content = $content -replace ([char]0x2192), '->'
  Set-Content -LiteralPath $pdfInput -Value $content -Encoding UTF8

  $arguments = Get-PandocPdfArguments -Context $Context
  Invoke-Pandoc -Context $Context -Arguments $arguments

  if (Test-Path $Context.PdfOutputFile) {
    Write-Log "  [OK] $(ConvertTo-RelativePath $Context.PdfOutputFile)" Success
  }
  else {
    throw "PDF output not created: $($Context.PdfOutputFile)"
  }
}

