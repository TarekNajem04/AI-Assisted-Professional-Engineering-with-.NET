# مرجع المعاملات

## Export-Manuscript.ps1

نقطة الدخول الرئيسية لنظام التصدير.

### `-SourceFile` (إلزامي)

مسار ملف Markdown المصدر المراد تصديره. يقبل المسارات المطلقة والنسبية
(نسبة إلى جذر المستودع).

```powershell
-SourceFile "book/chapters/Chapter-01/assembled/chapter-01.en.md"
```

اسم الملف يحدد اللغة (`en` أو `ar` suffix) وتصنيف المستند عبر قواعد
تخطيط المصدر في `export-config.json`.

### `-StyleProfile`

نمط العرض المراد تطبيقه. القيمة الافتراضية: `"oreilly"`.

```txt
-StyleProfile "default"
-StyleProfile "oreilly"
-StyleProfile "apress"
```

كل نمط يحدد قالب DOCX مرجعي خاص به (خطوط، ألوان، تباعد) وسمة PDF
(تخطيط، رؤوس، طباعته). الأنماط موجودة تحت
`scripts/export/styles/<profile-name>/`.

### `-PdfEngine`

محرك PDF الذي يُمرر إلى علم `--pdf-engine` في Pandoc. القيمة الافتراضية:
`"lualatex"`.

```powershell
-PdfEngine "lualatex"
```

حاليًا `lualatex` فقط مدعوم. هذا المعامل محتفظ به للتوسع المستقبلي —
إضافة محرك جديد يتطلب تحديث مصفوفة `PdfEngines` في `export-config.json`
و `ValidateSet` في هذا المعامل.

### `-RebuildMermaid`

إعادة إنشاء جميع رسوم Mermaid قسرًا حتى لو كانت مخبأة. يقبل `$true` أو
`$false`. عندما يكون `$null` (افتراضيًا)، تُستخدم قيمة الإعدادات
`Mermaid.Rebuild`.

```powershell
-RebuildMermaid $true
```

### `-RebuildCovers`

إعادة إنشاء صور الأغلفة قسرًا حتى لو كانت مخبأة. يقبل `$true` أو
`$false`. عندما يكون `$null` (افتراضيًا)، تُستخدم قيمة الإعدادات
`Covers.Rebuild`.

```powershell
-RebuildCovers $true
```

## Initialize-ExportEnvironment.ps1

يدير هيكل دليل `exports/` وملفات placeholder.

### `-Force`

تجاوز تأكيد التفاعل. إعادة تعيين كاملة: حذف جميع ملفات المخرجات وإعادة
إنشاء هيكل المجلدات مع ملفات placeholder.

```powershell
-Force
```

### `-CleanPlaceholdersOnly`

وضع غير مدمر يزيل `placeholder.txt` فقط من المجلدات التي تحتوي على ملفات
مخرجات حقيقية. ينشئ placeholders مفقودة في المجلدات الفارغة. هذا هو
الوضع المستخدم من قبل الأنبوب أثناء التصدير العادي.

```powershell
-CleanPlaceholdersOnly
```

## export-config.json

ملف الإعدادات المركزي في `scripts/export/export-config.json`.

| المفتاح | النوع | الوصف |
|---------|-------|-------|
| `ValidLanguages` | `string[]` | اللغات المدعومة: `["ar", "en"]` |
| `ValidTypes` | `string[]` | أنواع المخرجات: `["pdf", "docx"]` |
| `ValidCategories` | `string[]` | تصنيفات المستندات: `["chapters", "sections", "manifesto", "tests"]` |
| `PdfEngines` | `string[]` | محركات PDF المسجلة: `["lualatex"]` |
| `OutputRoot` | `string` | دليل المخرجات الجذر: `"exports"` |
| `Pdf.Engine` | `string` | محرك PDF الافتراضي: `"lualatex"` |
| `Pdf.FontSize` | `string` | حجم الخط الأساسي: `"12pt"` |
| `Pdf.PageSize` | `string` | حجم الصفحة: `"a4"` |
| `Pdf.Margins` | `object` | هوامش الصفحة (Top, Bottom, Inner, Outer, BindingOffset) |
| `Pdf.Fonts` | `object` | تعريفات الخطوط (MainEn, MainAr, Sans, Mono) |
| `Pdf.LineHeight` | `number` | مضاعف ارتفاع السطر: `1.05` |
| `Pdf.SyntaxHighlighting` | `string` | نمط تظليل التركيب: `"pygments"` |
| `Pdf.CoverPages` | `bool` | تضمين صفحات الأغلفة في PDF: `true` |
| `Pdf.Title` | `string` | عنوان المستند للبيانات الوصفية |
| `Docx.SyntaxHighlighting` | `string` | نمط تظليل DOCX: `"pygments"` |
| `Docx.ReferenceDoc` | `string` | اسم ملف قالب DOCX المرجعي |
| `Docx.CoverPages` | `bool` | تضمين صفحات الأغلفة في DOCX: `true` |
| `Docx.Margins` | `object` | هوامش DOCX (Top, Bottom, Left, Right) بالسنتيمتر |
| `Covers` | `object` | قوالب صور الأغلفة والأبعاد وإعدادات الجودة |
| `Mermaid` | `object` | إعدادات رسوم Mermaid (العرض، الخلفية، دليل التخزين المؤقت) |
| `Export.CleanOutput` | `bool` | حذف المخرجات الموجودة قبل التصدير: `true` |
| `Export.Cache` | `object` | إعدادات دليل التخزين المؤقت |
| `Log.Colors` | `object` | توزيع ألوان سجل الطرفية |
