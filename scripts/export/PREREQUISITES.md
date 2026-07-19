# Prerequisites for `Export-Manuscript.ps1`

All tools must be installed and available on `PATH` (or at the default paths listed below). Run the script without arguments to verify detection.

---

## Required

| Tool | Version | Install | Verified by |
|------|---------|---------|-------------|
| **Pandoc** | ≥ 2.19 | [pandoc.org/installing.html](https://pandoc.org/installing.html) | `Get-Command pandoc` |
| **PowerShell** | ≥ 7.0 | Built-in on Windows 11 | `$PSVersionTable.PSVersion` |

## PDF Engines (at least one)

### 1. wkhtmltopdf (fallback, easiest)

| Tool | Default path | Install |
|------|-------------|---------|
| `wkhtmltopdf.exe` | `C:\Program Files\wkhtmltopdf\bin\wkhtmltopdf.exe` | [wkhtmltopdf.org/downloads.html](https://wkhtmltopdf.org/downloads.html) |
| `wkhtmltoimage.exe` | `C:\Program Files\wkhtmltopdf\bin\wkhtmltoimage.exe` | (bundled with wkhtmltopdf) |

Used for: cover JPG generation, PDF fallback when XeLaTeX is missing.

### 2. XeLaTeX (default, best quality)

| Component | Details |
|-----------|---------|
| **Distribution** | MiKTeX (Windows) or TeX Live |
| **Engine** | `xelatex.exe` (called via Pandoc `--pdf-engine`) |
| **Packages** | `libertinus`, `microtype`, `titlesec`, `titletoc`, `fancyhdr`, `booktabs`, `tabularx`, `array`, `caption`, `enumitem`, `tcolorbox`, `framed`, `etoolbox`, `textcomp`, `eurosym`, `mathspec`, `hyperref`, `xcolor`, `geometry`, `setspace`, `ragged2e`, `float`, `listings` (or `minted`), `iftex`, `unicode-math` |

Install:
- **MiKTeX** (Windows): <https://miktex.org/download> — packages install on demand
- **TeX Live**: <https://tug.org/texlive/> — then `tlmgr install libertinus libertinus-fonts`

## Diagram Rendering (optional, for Mermaid support)

| Tool | Install |
|------|---------|
| **Node.js** | [nodejs.org](https://nodejs.org/) |
| **@mermaid-js/mermaid-cli** | `npm install -g @mermaid-js/mermaid-cli` |

If `mmdc` is not found, Mermaid code blocks are kept as literal text.

## DOCX Reference Template

| Tool | Install |
|------|---------|
| **Python 3** | [python.org](https://python.org/) |
| **python-docx** | `pip install python-docx` |

Run `generate-reference-docx.py` once to produce `custom-reference.docx`.

---

## Quick install checklist (Windows)

```powershell
# 1. Pandoc
winget install Pandoc

# 2. wkhtmltopdf
winget install wkhtmltopdf

# 3. Node.js + Mermaid CLI
winget install OpenJS.NodeJS
npm install -g @mermaid-js/mermaid-cli

# 4. Python + python-docx
winget install Python.Python.3
pip install python-docx

# 5. MiKTeX (run installer from miktex.org)
#    Then launch MiKTeX Console → "Install on-the-fly" = Yes
```

After installing, run `Export-Manuscript.ps1 -Help` to verify all tools are detected.
