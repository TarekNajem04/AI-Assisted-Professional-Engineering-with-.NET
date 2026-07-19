<#
    SPDX-License-Identifier: MIT
    Copyright (c) 2025 Tarek Najem

    This file is part of the AI-Assisted Professional Engineering with .NET
    book project (https://github.com/TarekNajem/AI-Assisted-Professional-Engineering-With-.NET).
    It is subject to the terms and conditions of the MIT License as published
    in the LICENSE file at the root of this repository.

    This header must not be removed or modified without preserving the
    LICENSE file reference and copyright notice.

    Description:
        Renders Mermaid diagram blocks in the markdown source to PNG images.
        Each diagram is identified by a content hash for caching. After
        generation, computes {width=Xcm} so Pandoc renders the image at
        the correct physical size within the DOCX page content area.
#>
Set-StrictMode -Version Latest

function Invoke-MermaidPreprocessor {
  param([object]$Context)
  $insideRTL = $false
  if ($Context.Language -eq 'ar') { $insideRTL = $true }

  $mermaidConfig = $Context.Config.Mermaid
  $cacheDir = Join-Path $Context.RepositoryRoot $mermaidConfig.CacheDir
  New-Item -ItemType Directory -Force -Path $cacheDir | Out-Null

  $mdPath = $Context.Working.Markdown
  if (-not (Test-Path $mdPath)) { return }
  $sourceContent = Get-Content -LiteralPath $mdPath -Raw -Encoding UTF8
  $pattern = '```mermaid\s*(.*?)\s*```'
  $matches = [regex]::Matches($sourceContent, $pattern, 'Singleline')
  if ($matches.Count -eq 0) { return }

  $Context.Mermaid.Diagrams = [System.Collections.ArrayList]@()
  $Context.Mermaid.HasMermaid = $true

  $rendered = @()

  function Get-DiagramHash([string]$Code) {
    $sha = [System.Security.Cryptography.SHA256]::Create()
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Code)
    $hashBytes = $sha.ComputeHash($bytes)
    return -join ($hashBytes | ForEach-Object { '{0:x2}' -f $_ })
  }

  foreach ($match in $matches) {
    $MermaidCode = $match.Groups[1].Value.Trim()
    $MermaidCode = $MermaidCode -replace '^[a-zA-Z]+="[^"]*"\s*', ''
    $hash = Get-DiagramHash $MermaidCode
    $DiagramId = "dg-$($hash.Substring(0, 12))"

    $Context.Mermaid.Diagrams += $DiagramId

    $TempMmd = Join-Path $Context.Working.Directory "$DiagramId.mmd"
    $CachedImage = Join-Path $cacheDir "$DiagramId.png"
    $ImagePath = Join-Path $Context.Working.Directory "$DiagramId.png"

    Set-Content -LiteralPath $TempMmd -Value $MermaidCode -Encoding UTF8

    $RebuildImage = $mermaidConfig.Rebuild -eq $true
    $IsImageExist = Test-Path -LiteralPath $CachedImage -PathType Leaf
    if ($RebuildImage -and $IsImageExist) {
      Remove-Item -LiteralPath $CachedImage -Force
      Write-Log "  [Deleted Mermaid] $(ConvertTo-RelativePath $CachedImage)" Deleted
      $IsImageExist = $false
    }

    if ($IsImageExist) {
      Copy-Item -LiteralPath $CachedImage -Destination $ImagePath -Force
      Write-Log "  [Cached Mermaid] $(ConvertTo-RelativePath $CachedImage)" Muted
    }
    else {
      $mmdc = $Context.Tools.MermaidCli
      $width = $mermaidConfig.Width
      $bg = $mermaidConfig.Background
      $mmdcArgs = @('-i', $TempMmd, '-o', $CachedImage)
      if ($width -gt 0) { $mmdcArgs += '-w'; $mmdcArgs += "$width" }
      if ($bg) { $mmdcArgs += '-b'; $mmdcArgs += $bg }
      & $mmdc @mmdcArgs 2>&1 | Out-Null

      if ($LASTEXITCODE -ne 0) {
        Write-Log "  [WARN] mmdc failed for $DiagramId — removing block from source" Warning
        $sourceContent = $sourceContent -replace [regex]::Escape($match.Value), ''
        continue
      }

      Copy-Item -LiteralPath $CachedImage -Destination $ImagePath -Force
      Write-Log "  [Generated Mermaid] $(ConvertTo-RelativePath $CachedImage)" Info
    }

    $relPath = ConvertTo-RelativePath $CachedImage
    $replacement = "![Diagram: $DiagramId]($relPath)"
    $sourceContent = $sourceContent -replace [regex]::Escape($match.Value), $replacement

    $rendered += [pscustomobject]@{
      Id    = $DiagramId
      Input = $TempMmd
      Image = $ImagePath
    }
  }

  Set-Content -LiteralPath $mdPath -Value $sourceContent -Encoding UTF8

  $Context.Mermaid.Diagrams = [System.Collections.ArrayList]@($rendered)
}
