Set-StrictMode -Version Latest

Describe "MermaidPreprocessor" {

    BeforeAll {

        . "$PSScriptRoot/../../../scripts/export/preprocessors/MermaidPreprocessor.ps1"

    }

    It "Extracts Mermaid blocks" {

        $context = [pscustomobject]@{

            WorkingMarkdown = @"
# Title

```{.mermaid #graph-01}
graph TD
A --> B
````

"@

```
        Tools = [pscustomobject]@{

            MermaidCli = $null

        }

    }

    $processor =
        [MermaidPreprocessor]::new()

    $processor.Execute($context)

    $context.HasMermaid |
        Should -BeTrue

    $context.Mermaid.Count |
        Should -Be 1

    $context.Mermaid[0].Id |
        Should -Be "graph-01"

    $context.Mermaid[0].Body.Count |
        Should -Be 2

}
```

}