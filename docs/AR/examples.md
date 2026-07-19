# أمثلة عملية

## مثال 1: تصدير فصل كتاب كامل (إنجليزي، نمط O'Reilly)

```powershell
pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
    -SourceFile "book/chapters/Chapter-01/assembled/chapter-01.en.md" `
    -StyleProfile "oreilly"
```

المخرجات:

```txt
exports/pdf/en/chapters/chapter-01.en.pdf
exports/docx/en/chapters/chapter-01.en.docx
```

## مثال 2: تصدير فصل عربي (النمط الافتراضي)

```powershell
pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
    -SourceFile "book/chapters/Chapter-01/assembled/chapter-01.ar.md" `
    -StyleProfile "default"
```

المخرجات العربية تتضمن رأس `arabic-setup.tex` للتنضيد الصحيح من اليمين
لليسار باستخدام `polyglossia`.

## مثال 3: تصدير مع إعادة بناء Mermaid قسرًا

```powershell
pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
    -SourceFile "book/chapters/Chapter-01/assembled/chapter-01.en.md" `
    -StyleProfile "apress" `
    -RebuildMermaid $true
```

هذا يعيد إنشاء جميع صور رسوم Mermaid PNG حتى لو كانت موجودة في
ذاكرة التخزين المؤقت.

## مثال 4: تصدير فصول متعددة

التكرار عبر جميع الفصول الإنجليزية المجمعة:

```powershell
$files = Get-ChildItem "book/**/assembled/*.en.md" -Recurse
foreach ($file in $files) {
    pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
        -SourceFile $file.FullName `
        -StyleProfile "oreilly"
}
```

## مثال 5: تصدير مخطوطة فصل

```powershell
pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
    -SourceFile "book/sections/introduction/assembled/introduction.en.md" `
    -StyleProfile "oreilly"
```

المخرجات توضع في `exports/pdf/en/sections/` و `exports/docx/en/sections/`.

## مثال 6: إعادة تعيين كاملة لدليل التصدير

```powershell
pwsh -ExecutionPolicy Bypass -File scripts\export\tools\Initialize-ExportEnvironment.ps1 -Force
```

هذا يحذف جميع ملفات PDF و DOCX والرسوم والأغلفة الموجودة مع الحفاظ على
هيكل الدليل وملفات placeholder.

## مثال 7: التحقق من ملفات المخرجات

بعد التصدير، تحقق من الملفات:

```powershell
Get-ChildItem exports/pdf -Recurse -Filter *.pdf
Get-ChildItem exports/docx -Recurse -Filter *.docx
```

## مثال 8: التشغيل مع بيئة نظيفة

أنبوب كامل مع إعادة تعيين البيئة قبل التصدير:

```powershell
pwsh -ExecutionPolicy Bypass -File scripts\export\tools\Initialize-ExportEnvironment.ps1 -Force
pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
    -SourceFile "book/chapters/Chapter-01/assembled/chapter-01.en.md" `
    -StyleProfile "oreilly"
```

## مثال 9: تفصيل معالجة DOCX بعد الإنتاج

عند توليد DOCX، يشغل الأنبوب خطوات المعالجة التالية بعد الإنتاج:

1. تطبيع ألوان التظليل (عبر `docx-apply-hl-colors.py`)
2. ملاءمة الصور لأبعاد الصفحة (عبر `docx-fit-images.py`)
3. ضبط هوامش المستند من الإعدادات (عبر `docx-set-margins.py`)
4. إضافة صور الغلاف الأمامي والخلفي (عبر `docx-add-covers.py`)

كل خطوة تُشغل فقط إذا كان سكريبت بايثون المقابل موجودًا.

## مثال 10: مخرجات PDF باستخدام LuaLaTeX

توليد PDF يستخدم ميزات LuaLaTeX التالية:

| الميزة        | التنفيذ                                                     |
|---------------|-------------------------------------------------------------|
| خطوط OpenType | حزمة `fontspec` مع `\setmainfont`، `\sansfont`، `\monofont` |
| النص العربي   | `polyglossia` مع `\setmainlanguage[locale=morocco]{arabic}` |
| كتل الأكواد    | `tcolorbox` أو `minted` لأكواد مظللة التركيب                 |
| أكواد Bidi    | `\textdir TLT` في كتل الأكواد عبر مرشح Lua                   |
| هندسة الصفحة  | حزمة `geometry` مع تعويض التجليد لتخطيط الكتاب              |
