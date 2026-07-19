Set-StrictMode -Version Latest

function Get-PandocDocxArguments {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory)]
    $Context
  )

  $hl = if ($Context.DocxSyntaxHighlighting) { $Context.DocxSyntaxHighlighting } `
    elseif ($null -ne $Context.Config.Docx.PSObject.Properties['SyntaxHighlighting'] -and $Context.Config.Docx.SyntaxHighlighting) { $Context.Config.Docx.SyntaxHighlighting } `
    else { 'default' }
  $arguments = Get-PandocCommonArguments -Context $Context -SyntaxHighlighting $hl

  $arguments += "--to=docx"
  $arguments += "--dpi=96"

  if (Test-Path $Context.TemplateFilePath) {
    $arguments += "--reference-doc=$($Context.TemplateFilePath)"
  }

  $arguments += "--output"
  $arguments += $Context.DocxOutputFile

  return $arguments
}
