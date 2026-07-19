Set-StrictMode -Version Latest

Describe "MermaidPreprocessor.GetBlocks" {

    BeforeAll {

        . "$PSScriptRoot/../../../scripts/export/preprocessors/MermaidPreprocessor.ps1"

        $processor =
            [MermaidPreprocessor]::new()

    }

    It "Returns no blocks when markdown contains no Mermaid" {

        $blocks =
            $processor.GetBlocks(@"

# Title

Some text.

"@)

        $blocks.Count |
            Should -Be 0

    }

    It "Extracts one Mermaid block" {

        $blocks =
            $processor.GetBlocks(@"

```{.mermaid #graph-01}
graph TD
A --> B
````

"@)

```
    $blocks.Count |
        Should -Be 1

    $blocks[0].Id |
        Should -Be "graph-01"

    ($blocks[0].Body -join "`n") |
        Should -Be @"
```

graph TD
A --> B

"@.Trim()

```
}

It "Extracts multiple Mermaid blocks" {

    $blocks =
        $processor.GetBlocks(@"
```

```{.mermaid #one}
graph TD
A --> B
```

Some text.

```{.mermaid #two}
graph TD
B --> C
```

"@)

```
    $blocks.Count |
        Should -Be 2

    $blocks[0].Id |
        Should -Be "one"

    $blocks[1].Id |
        Should -Be "two"

}
```

}

