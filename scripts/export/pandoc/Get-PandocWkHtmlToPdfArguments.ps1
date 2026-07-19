<#
.SYNOPSIS
    Builds Pandoc arguments for wkhtmltopdf.
#>
###############################################################################
# DEPRECATED
#
# This file was restored after the Export vNext migration.
#
# It is intentionally kept as a historical implementation and for future
# architectural review.
#
# This file is NOT part of the active export pipeline.
#
# Do NOT remove or modify this file unless an explicit repository cleanup
# decision has been made.
#
###############################################################################

Set-StrictMode -Version Latest

function Get-PandocWkHtmlToPdfArguments {

  [CmdletBinding()]
  param(
    [Parameter(Mandatory)]
    $Context
  )

  $arguments = @()

  $arguments += $Context.Working.Markdown

  $arguments += "--from=markdown"

  $arguments += "--to=html5"

  $arguments += "--standalone"

  $arguments += "--pdf-engine=wkhtmltopdf"

  $arguments += "--output"
  $arguments += $Context.Output.Pdf.File

  foreach ($filter in $Context.Pandoc.LuaFilters) {

    $arguments += "--lua-filter"
    $arguments += $filter

  }

  foreach ($path in $Context.Pandoc.ResourcePaths) {

    $arguments += "--resource-path"
    $arguments += $path

  }

  return $arguments

}


