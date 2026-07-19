# أنماط النشر

## نظرة عامة

أنماط النشر هي مجموعات مكتفية ذاتيًا من القوالب والمولدات التي تحدد
المظهر المرئي لمستندات PDF و DOCX المصدرة. كل نمط موجود في دليله الخاص
تحت `scripts/export/styles/`.

## الأنماط المدمجة

| النمط | خط المتن | خط العناوين | خط الأكواد | الخاصية المميزة |
|-------|----------|-------------|------------|------------------|
| `default` | EB Garamond | Source Sans Pro | Consolas | كتاب احترافي كلاسيكي |
| `oreilly` | Noto Serif | Open Sans | Consolas | أكواد داكنة، لمسات زرقاء |
| `apress` | Palatino Linotype | Segoe UI | Consolas | رمادي محافظ |

## هيكل دليل النمط

كل نمط يتبع هذا الهيكل:

```txt
styles/<profile-name>/
├── profile.json              # بيانات النمط ومراجع المولدات
├── pdf-theme.tex             # قالب سمة LaTeX (مخرجات PDF)
├── generate-docx-style.py    # مولد مستند DOCX المرجعي
├── generate-pdf-style.py     # مولد سمة PDF
└── docx-validate-styles.py   # سكريبت التحقق من أنماط DOCX
```

## profile.json

ملف تعريف النمط. مثال (`oreilly/profile.json`):

```json
{
  "name": "oreilly",
  "description": "O'Reilly-inspired professional book style — Noto Serif body, Open Sans headings, dark code blocks, deep blue accents",
  "version": "1.0",
  "docx": {
    "generator": "generate-docx-style.py",
    "syntaxHighlighting": "breezedark"
  },
  "pdf": {
    "generator": "generate-pdf-style.py"
  }
}
```

الحقول:

- `name`: معرف النمط يطابق اسم الدليل.
- `description`: وصف مقروء.
- `version`: إصدار مخطط النمط.
- `docx.generator`: سكريبت بايثون لإنشاء قالب DOCX المرجعي.
- `docx.syntaxHighlighting`: نمط تظليل التركيب (مثل `pygments`،
  `breezedark`، أو مسار ملف).
- `pdf.generator`: سكريبت بايثون لتوليد ملف سمة PDF.

## كيفية تطبيق الأنماط

عند تشغيل `Invoke-StyleProfile.ps1`:

1. تحميل `profile.json` من دليل النمط.
2. تشغيل `generate-docx-style.py` → ينتج `custom-reference.docx`.
3. تشغيل `generate-pdf-style.py` → ينتج `pdf-theme.tex` في دليل العمل.
4. تحديث `Context.StyleProfileName` و `Context.DocxSyntaxHighlighting`.

الملفات المولدة تُستخدم بعد ذلك بواسطة `Get-PandocDocxArguments.ps1`
(لعلم `--reference-doc`) و `Get-PandocPdfArguments.ps1` (لتضمين رأس
السمة).

## كيفية إضافة نمط نشر جديد

1. أنشئ دليلًا جديدًا: `styles/<new-profile>/`

2. انسخ الملفات من نمط موجود (مثل `default/`).

3. حرر `profile.json`:
   - عيّن `name` لاسم النمط الخاص بك.
   - حدث `description`.
   - اضبط `docx.syntaxHighlighting` إذا لزم الأمر (مثل `"monochrome"`).

4. حرر `generate-docx-style.py`:
   - غير عائلات الخطوط وأحجامها وألوانها.
   - عدل تباعد الفقرات والمسافات البادئة.
   - اضبط أنماط العناوين والترقيم.
   - حدث تنسيق كتل الأكواد.

5. حرر `generate-pdf-style.py`:
   - غير عائلات الخطوط عبر `\setmainfont`، `\sansfont`، `\monofont`.
   - اضبط هندسة الصفحة والهوامش.
   - عدل تنسيق العناوين والألوان.
   - حدث مظهر كتل الأكواد.

6. حرر `pdf-theme.tex` إذا كنت بحاجة إلى تخصيص LaTeX إضافي:
   - محتوى الرأس والتذييل.
   - تخطيط جدول المحتويات.
   - نمط ترقيم الصفحات.
   - تنسيق التسميات التوضيحية.

7. اختبر النمط:

   ```powershell
   pwsh -ExecutionPolicy Bypass -File scripts/export/Export-Manuscript.ps1 `
       -SourceFile "book/chapters/Chapter-01/assembled/chapter-01.en.md" `
       -StyleProfile "new-profile"
   ```
