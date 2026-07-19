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

function Create-ApplicationContext {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory)]
    [string]$SourceFile,
    [Parameter(Mandatory)]
    [string]$RepositoryRoot,
    [string]$PdfEngine,
    [string]$Workspace
  )

  $configPath = Join-Path $RepositoryRoot 'scripts/export/export-config.json'
  if (-not (Test-Path $configPath)) { throw "Config not found: $configPath" }
  $raw = Get-Content -LiteralPath $configPath -Raw -Encoding UTF8
  $config = [pscustomobject](ConvertFrom-Json $raw)

  if (-not $PdfEngine) { $PdfEngine = $config.Pdf.Engine }
  if ($config.PdfEngines -and $config.PdfEngines.Count -gt 0) {
    if ($PdfEngine -notin $config.PdfEngines) {
      Write-Warning "Unsupported PDF engine '$PdfEngine'. Falling back to '$($config.Pdf.Engine)'. Supported engines: $($config.PdfEngines -join ', ')"
      $PdfEngine = $config.Pdf.Engine
    }
  }
  if (-not $Workspace) { $Workspace = Join-Path $env:TEMP "book-export-$(New-Guid)" }

  . "$RepositoryRoot\scripts\export\core\Export-TargetResolver.ps1"
  $resolved = Resolve-ExportTarget `
    -SourceFile $SourceFile `
    -RepositoryRoot $RepositoryRoot `
    -ValidLanguages $config.ValidLanguages `
    -SourceLayouts $config.SourceLayouts

  $baseName = "$($resolved.BaseName).$($resolved.Language)"
  $language = $resolved.Language
  $category = $resolved.Kind

  $scriptsRoot = Join-Path $RepositoryRoot 'scripts/export'

  $pdfOutput = Join-Path $RepositoryRoot "$($config.Export.OutputRoot)/pdf/$language/$category/$baseName.pdf"
  $docxOutput = Join-Path $RepositoryRoot "$($config.Export.OutputRoot)/docx/$language/$category/$baseName.docx"

  . "$RepositoryRoot\scripts\export\project\Project-Tools.ps1"
  $tools = Get-ProjectTools

  $templateDir = Join-Path $RepositoryRoot 'scripts/export'
  $templateFileName = $config.TemplateFileName
  $templateFilePath = Join-Path $templateDir $templateFileName

  $layouts = [SourceLayoutEntry[]]@()
  foreach ($entry in $config.SourceLayouts) {
    $layouts += [SourceLayoutEntry]@{ Regex = $entry.Regex; Category = $entry.Category }
  }

  $ctx = [ApplicationContext]@{
    RepositoryRoot = $RepositoryRoot
    SourceFile = (Resolve-Path $SourceFile).Path
    BaseName = $baseName
    Language = $language
    Category = $category
    PdfOutputFile = $pdfOutput
    DocxOutputFile = $docxOutput
    PdfEngine = $PdfEngine
    ConfigPath = $configPath
    Config = $config
    TemplateFileName = $templateFileName
    TemplateFilePath = $templateFilePath
    ValidLanguages = [string[]]@($config.ValidLanguages)
    ValidTypes = [string[]]@($config.ValidTypes)
    ValidCategories = [string[]]@($config.ValidCategories)
    PdfEngines = [string[]]@($config.PdfEngines)
    SourceLayouts = $layouts
    Mermaid = [MermaidState]@{
      Diagrams = [System.Collections.ArrayList]@()
      HasMermaid = $false
    }
    CSharp = [CSharpState]@{ BlockCount = 0 }
    Cover = [CoverState]@{ Front = $null; Back = $null; HasCovers = $false; HeaderFile = $null }
    Tools = $tools
    StyleProfileName = "default"
    DocxSyntaxHighlighting = $null
    Working = [WorkingState]@{ Directory = $Workspace; Markdown = (Resolve-Path $SourceFile).Path; ThemeFile = $null }
    Paths = [ScriptsPaths]@{
      LatexDir = Join-Path $scriptsRoot 'latex'
      LuaDir = Join-Path $scriptsRoot 'lua'
      HtmlDir = Join-Path $scriptsRoot 'html'
      ThemeFile = Join-Path (Join-Path $scriptsRoot 'latex') 'pdf-theme.tex'
      ArabicFile = Join-Path (Join-Path $scriptsRoot 'latex') 'arabic-setup.tex'
      CoversHeaderFile = Join-Path (Join-Path $scriptsRoot 'latex') 'cover-paths.tex'
      DocxReferenceTemplate = $templateFilePath
      LuaFilterCodeblockBidi = Join-Path (Join-Path $scriptsRoot 'lua') 'codeblock-bidi.lua'
      LuaFilterEscapeBackslash = Join-Path (Join-Path $scriptsRoot 'lua') 'escape-backslash.lua'
      ValidScriptPath = Join-Path $scriptsRoot 'docx-validate-styles.py'
      MarginScriptPath = Join-Path $scriptsRoot 'docx-set-margins.py'
      CoversScriptPath = Join-Path $scriptsRoot 'docx-add-covers.py'
      StylesDir = Join-Path $scriptsRoot 'styles'
    }
  }

  if ($ctx.Config.Pdf.FontSize -notmatch 'pt$') { $ctx.Config.Pdf.FontSize = '12pt' }

  return $ctx
}

