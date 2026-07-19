# محركات PDF

## المحرك الحالي

يستخدم النظام **LuaLaTeX** كمحرك PDF وحيد. يتم استدعاؤه عبر علم
`--pdf-engine` في Pandoc.

```powershell
--pdf-engine=lualatex
```

تم اختيار LuaLaTeX للأسباب التالية:

- دعم كامل لخطوط OpenType عبر `fontspec` (للنصوص العربية واللاتينية).
- `polyglossia` للتنضيد متعدد اللغات ودعم bidi.
- تكامل ناضج مع Pandoc.
- تنضيد PDF احترافي ومتسق.

## كيفية تدفق إعداد المحرك

1. **الإعدادات**: `export-config.json` يحدد `PdfEngines: ["lualatex"]`
   والافتراضي `Pdf.Engine: "lualatex"`.

2. **إنشاء السياق**: `Create-ApplicationContext.ps1` يقرأ الإعداد الافتراضي
   ويتحقق من المحرك مقابل قائمة `PdfEngines`.

3. **باني المعاملات**: `Get-PandocPdfArguments.ps1` يقرأ
   `$Context.PdfEngine` ويمرره إلى Pandoc:

   ```powershell
   $arguments += "--pdf-engine=$($Context.PdfEngine)"
   ```

4. **التنفيذ**: `Invoke-Pandoc.ps1` يستدعي Pandoc مع المعاملات المجمعة،
   بما في ذلك المحرك المختار.

## كيفية إضافة محرك PDF جديد

الهندسة المعمارية مصممة للتوسع. لإضافة محرك جديد:

### الخطوة 1: تسجيل المحرك

أضف اسم المحرك إلى مصفوفة `PdfEngines` في `export-config.json`:

```json
"PdfEngines": ["lualatex", "context"]
```

حدث `ValidateSet` في `Export-Manuscript.ps1`:

```powershell
[ValidateSet('lualatex', 'context')]
[string]$PdfEngine = 'lualatex',
```

### الخطوة 2: إنشاء باني معاملات

أنشئ `scripts/export/pandoc/Get-PandocContextArguments.ps1` متبعًا النمط
الموجود:

```powershell
function Get-PandocContextArguments {
  [CmdletBinding()]
  param($Context)

  $arguments = @()
  $arguments += $Context.Working.Markdown
  $arguments += "--from=markdown"
  $arguments += "--to=pdf"
  $arguments += "--pdf-engine=context"
  $arguments += "--output"
  $arguments += $Context.PdfOutputFile
  return $arguments
}
```

### الخطوة 3: تحديث توزيع PDF

اسم المحرك المخزن في `$Context.PdfEngine` يُمرر بشكل عام إلى
`Get-PandocPdfArguments.ps1`، الذي يستخدمه ديناميكيًا عبر
`--pdf-engine=$($Context.PdfEngine)`. إذا كان المحرك الجديد يتطلب مجموعة
مختلفة من الأعلام، أضف توزيعًا شرطيًا في `Invoke-Pdf.ps1`.

### الخطوة 4: الاختبار

```powershell
pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
    -SourceFile "book/chapters/Chapter-01/assembled/chapter-01.en.md" `
    -PdfEngine "context"
```

## المحرك القديم: wkhtmltopdf

ملف `Get-PandocWkHtmlToPdfArguments.ps1` موجود في دليل `pandoc/` كمرجع
تاريخي. هو موسوم بـ **مهمل (DEPRECATED)** وليس جزءًا من الأنبوب النشط.
محتفظ به للمراجعة الهندسية ولن يُحذف إلا بقرار مشروع صريح.
