<#
.SYNOPSIS
    Mermaid configuration for the export pipeline.
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

function Get-ProjectMermaidConfiguration {

  [CmdletBinding()]
  param()

  return [pscustomobject]@{

    Width = 800

    Background = "transparent"

  }

}


