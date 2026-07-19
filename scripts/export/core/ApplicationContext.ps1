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
class MermaidState {
  [System.Collections.ArrayList] $Diagrams
  [bool] $HasMermaid
  MermaidState() { $this.Diagrams = [System.Collections.ArrayList]@(); $this.HasMermaid = $false }
}

class CSharpState {
  [int] $BlockCount
  CSharpState() { $this.BlockCount = 0 }
}

class CoverState {
  [string] $Front
  [string] $Back
  [bool] $HasCovers
  [string] $HeaderFile
  CoverState() { $this.Front = $null; $this.Back = $null; $this.HasCovers = $false; $this.HeaderFile = $null }
}

class ToolsState {
  [string] $Pandoc
  [string] $MermaidCli
  [string] $ImageMagick
}

class WorkingState {
  [string] $Directory
  [string] $Markdown
  [string] $ThemeFile
}

class ScriptsPaths {
  [string] $LatexDir
  [string] $LuaDir
  [string] $HtmlDir
  [string] $ThemeFile
  [string] $ArabicFile
  [string] $CoversHeaderFile
  [string] $DocxReferenceTemplate
  [string] $LuaFilterCodeblockBidi
  [string] $LuaFilterEscapeBackslash
  [string] $ValidScriptPath
  [string] $MarginScriptPath
  [string] $CoversScriptPath
  [string] $StylesDir
}

class SourceLayoutEntry {
  [string] $Regex
  [string] $Category
}

class ApplicationContext {
  [string] $RepositoryRoot
  [string] $SourceFile
  [string] $BaseName
  [string] $Language
  [string] $Category
  [string] $PdfOutputFile
  [string] $DocxOutputFile
  # Active PDF engine (e.g. "lualatex"). Must be one of $PdfEngines.
  # To add a new engine in the future:
  #   1. Append its name to PdfEngines in export-config.json
  #   2. Create corresponding argument builder (e.g. Get-PandocFooArguments.ps1)
  #   3. Update the dispatch logic in Invoke-Pdf or Invoke-Pandoc
  [string] $PdfEngine
  [string] $ConfigPath
  [string] $TemplateFileName
  [string] $TemplateFilePath
  [string[]] $ValidLanguages
  [string[]] $ValidTypes
  [string[]] $ValidCategories
  # Registry of all known PDF engines (from config). This is the extension point:
  # adding a new name here + creating the argument builder is the intended pattern.
  [string[]] $PdfEngines
  [SourceLayoutEntry[]] $SourceLayouts
  [string] $StyleProfileName
  [string] $DocxSyntaxHighlighting

  [pscustomobject] $Config

  [MermaidState] $Mermaid
  [CSharpState] $CSharp
  [CoverState] $Cover
  [ToolsState] $Tools
  [WorkingState] $Working
  [ScriptsPaths] $Paths
}

