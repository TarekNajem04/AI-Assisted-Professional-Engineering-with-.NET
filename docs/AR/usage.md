# دليل الاستخدام

## المتطلبات الأساسية

- **Pandoc** (>= 3.1)
- **LuaLaTeX** (TeX Live أو MiKTeX مع حزم `fontspec` و `polyglossia`
  و `libertinus`)
- **Python 3** (لمولدات الأنماط ومعالجة DOCX)
- **Node.js** (لـ Mermaid CLI: `@mermaid-js/mermaid-cli`)
- **ImageMagick** (لمعالجة صور الأغلفة)
- **PowerShell 7+** (`pwsh`)

## بداية سريعة

تصدير فصل واحد بنمط النشر الافتراضي:

```powershell
pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
    -SourceFile "book/chapters/Chapter-01/assembled/chapter-01.en.md"
```

## سير العمل الكامل

### 1. تصدير فصل واحد

فصل إنجليزي بنمط O'Reilly:

```powershell
pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
    -SourceFile "book/chapters/Chapter-01/assembled/chapter-01.en.md" `
    -StyleProfile "oreilly"
```

فصل عربي بنمط Apress:

```powershell
pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
    -SourceFile "book/chapters/Chapter-01/assembled/chapter-01.ar.md" `
    -StyleProfile "apress"
```

### 2. التحكم في القطع المخبأة

إعادة بناء رسوم Mermaid قسرًا:

```powershell
pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
    -SourceFile "book/chapters/Chapter-01/assembled/chapter-01.en.md" `
    -RebuildMermaid $true
```

إعادة بناء صور الأغلفة قسرًا:

```powershell
pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
    -SourceFile "book/chapters/Chapter-01/assembled/chapter-01.en.md" `
    -RebuildCovers $true
```

### 3. اختيار نمط النشر

ثلاثة أنماط متاحة:

| النمط | الوصف |
|-------|-------|
| `default` | نمط كتاب احترافي — خط EB Garamond للمتن، Source Sans Pro للعناوين |
| `oreilly` | مستوحى من O'Reilly — خط Noto Serif للمتن، أكواد داكنة، لمسات زرقاء |
| `apress` | مستوحى من Apress — خط تقليدي serif للمتن، لوحة ألوان رمادية محافظة |

```powershell
-StyleProfile "oreilly"
```

### 4. هيكل المخرجات

الملفات المولدة توضع تحت `exports/`:

```txt
exports/
├── pdf/
│   ├── en/
│   │   └── chapters/
│   │       └── chapter-01.en.pdf
│   └── ar/
│       └── chapters/
│           └── chapter-01.ar.pdf
└── docx/
    ├── en/
    │   └── chapters/
    │       └── chapter-01.en.docx
    └── ar/
        └── chapters/
            └── chapter-01.ar.docx
```

## وضع الصامت / غير التفاعلي

النظام يعمل بشكل غير تفاعلي بالكامل افتراضيًا. السكربت
`Initialize-ExportEnvironment.ps1` يقبل `-Force -CleanPlaceholdersOnly` عند
استدعائه من الأنبوب:

```powershell
pwsh -ExecutionPolicy Bypass -File scripts\export\tools\Initialize-ExportEnvironment.ps1 `
    -Force -CleanPlaceholdersOnly
```

## تهيئة البيئة

لإعادة تعيين هيكل دليل التصدير (حذف جميع ملفات المخرجات باستثناء
placeholder):

```powershell
pwsh -ExecutionPolicy Bypass -File scripts\export\tools\Initialize-ExportEnvironment.ps1 -Force
```
