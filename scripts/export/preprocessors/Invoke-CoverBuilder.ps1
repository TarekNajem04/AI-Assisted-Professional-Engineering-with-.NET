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

function Invoke-CoverBuilder {
  param(
    [object]$Context
  )

  $coversConfig = $Context.Config.Covers
  $frontHtml = Join-Path $Context.RepositoryRoot $coversConfig.FrontTemplate
  $backHtml  = Join-Path $Context.RepositoryRoot $coversConfig.BackTemplate

  if (-not (Test-Path $frontHtml) -or -not (Test-Path $backHtml)) {
    $Context.Cover.HasCovers = $false
    $Context.Cover.HeaderFile = $null
    return
  }

  $coverDir = Join-Path $Context.RepositoryRoot 'exports/covers'
  New-Item -ItemType Directory -Force -Path $coverDir | Out-Null

  $fmt = $coversConfig.Format
  if (-not $fmt) { $fmt = 'jpg' }
  $frontImg = Join-Path $coverDir "front-cover.$fmt"
  $backImg  = Join-Path $coverDir "back-cover.$fmt"

  $Context.Cover.Front     = $frontImg
  $Context.Cover.Back      = $backImg
  $Context.Cover.HasCovers = $true

  $quality = $coversConfig.Quality
  if (-not $quality) { $quality = 85 }

  if ($coversConfig.Rebuild -eq $true) {
    foreach ($img in @($frontImg, $backImg)) {
      if (Test-Path $img) {
        Remove-Item -LiteralPath $img -Force
        Write-Log "  [Deleted Cover] $(ConvertTo-RelativePath $img)" Deleted
      }
    }
  }

  $renderer = $null
  $wk = Get-Command wkhtmltoimage -ErrorAction SilentlyContinue
  if ($wk) { $renderer = 'wkhtmltoimage' }

  function Render-HtmlToPng {
    param([string]$src, [string]$dst)
    if (Test-Path $dst) {
      $srcTime = (Get-Item $src).LastWriteTime
      $dstTime = (Get-Item $dst).LastWriteTime
      if ($srcTime -le $dstTime) {
        Write-Log "  [Cached Cover] $(ConvertTo-RelativePath $dst)" Muted
        return
      }
    }
    if ($renderer -eq 'wkhtmltoimage') {
      & wkhtmltoimage --width 794 --height 1123 --disable-smart-width --encoding UTF-8 --quality $quality $src $dst 2>&1 | Out-Null
      if ($LASTEXITCODE -ne 0) { throw "wkhtmltoimage failed for $src" }
      Write-Log "  [Generated Cover] $(ConvertTo-RelativePath $dst)" Info
    }
  }

  Render-HtmlToPng -src $frontHtml -dst $frontImg
  Render-HtmlToPng -src $backHtml  -dst $backImg

  $coverHeader = New-CoverHeaderFile -Context $Context
  $Context.Cover.HeaderFile = $coverHeader
}

function New-CoverHeaderFile {
  param([object]$Context)
  if (-not $Context.Cover.HasCovers) { return $null }
  $header = Join-Path $Context.Working.Directory 'cover-paths.tex'
  $wd = $Context.Working.Directory
  $frontLocal = Join-Path $wd 'front-cover.jpg'
  $backLocal  = Join-Path $wd 'back-cover.jpg'
  Copy-Item -LiteralPath $Context.Cover.Front -Destination $frontLocal -Force
  Copy-Item -LiteralPath $Context.Cover.Back  -Destination $backLocal  -Force
  $front = $frontLocal.Replace('\', '/')
  $back  = $backLocal.Replace('\', '/')
  @"
\newcommand{\coverfront}{$front}
\newcommand{\coverback}{$back}
"@ | Set-Content -LiteralPath $header -Encoding UTF8
  Write-Log "  [Cover Header] cover-paths.tex" Muted
  return $header
}

