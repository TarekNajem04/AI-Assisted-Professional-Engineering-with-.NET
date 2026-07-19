# الهندسة المعمارية

## نظرة عامة على الأنبوب

نظام التصدير هو نظام معياري يعتمد على مراحل، يعالج ملفات Markdown
المصدرية عبر 5 مراحل:

```txt
[1/5] تهيئة السياق
[2/5] تشغيل المعالجات الأولية
[3/5] توليد PDF
[4/5] توليد DOCX
[5/5] التنظيف
```

كل مرحلة مستقلة، وكائن السياق (`ApplicationContext`) يُمرر بينها لحمل
الإعدادات والحالة ومسارات الملفات.

## هيكل الدليل

```txt
scripts/export/
├── Export-Manuscript.ps1              # نقطة الدخول الرئيسية
├── export-config.json                 # الإعدادات المركزية
│
├── core/                              # تنسيق الأنبوب الأساسي
│   ├── ApplicationContext.ps1         # كائن بيانات حالة التصدير
│   ├── Create-ApplicationContext.ps1  # بناء السياق
│   ├── Export-TargetResolver.ps1      # اكتشاف اللغة والتصنيف
│   ├── Invoke-Preprocessor.ps1        # منسق المرحلة 2
│   ├── Invoke-Pdf.ps1                 # منسق المرحلة 3
│   ├── Invoke-Docx.ps1                # منسق المرحلة 4
│   ├── Invoke-Pandoc.ps1              # مغلف استدعاء Pandoc
│   ├── Invoke-StyleProfile.ps1        # محمل أنماط النشر
│   └── Validate-DocxStyles.ps1        # التحقق من أنماط DOCX
│
├── pandoc/                            # بناة معاملات Pandoc
│   ├── Get-PandocCommonArguments.ps1  # المعاملات المشتركة
│   ├── Get-PandocPdfArguments.ps1     # معاملات PDF
│   └── Get-PandocDocxArguments.ps1    # معاملات DOCX
│
├── preprocessors/                     # نصوص المعالجة الأولية
│   ├── MarkdownPreprocessor.ps1
│   ├── MermaidPreprocessor.ps1
│   ├── CSharpPreprocessor.ps1
│   └── Invoke-CoverBuilder.ps1
│
├── project/                           # المعرفة الخاصة بالمشروع
│   ├── Project-Conventions.ps1        # اكتشاف اللغة والنوع
│   ├── Project-Tools.ps1             # اكتشاف الأدوات الخارجية
│   └── Get-ProjectPandocConfiguration.ps1
│
├── shared/                            # الأدوات المساعدة المشتركة
│   ├── Validation.ps1                 # التحقق من السياق
│   ├── Workspace.ps1                  # إدارة الدليل المؤقت
│   └── Write-Log.ps1                  # تسجيل ملون
│
├── styles/                            # أنماط العرض
│   ├── default/                       # النمط الاحترافي الافتراضي
│   ├── oreilly/                       # نمط مستوحى من O'Reilly
│   └── apress/                        # نمط مستوحى من Apress
│
├── latex/                             # قوالب LaTeX
│   ├── pdf-theme.tex                  # قالب سمة PDF الأساسي
│   └── arabic-setup.tex               # إعدادات اللغة العربية ثنائية الاتجاه
│
├── lua/                               # مرشحات Pandoc Lua
│   ├── codeblock-bidi.lua             # معالجة كتل الأكواد RTL/LTR
│   └── escape-backslash.lua           # تخطي الخطوط المائلة العكسية لـ LaTeX
│
├── tools/                             # نصوص مساعدة
│   └── Initialize-ExportEnvironment.ps1
│
└── *.py                               # نصوص معالجة بايثون بعد الإنتاج
```

## شرح الملفات الرئيسية

### Export-Manuscript.ps1

نقطة الدخول الوحيدة. يدير جميع المراحل الخمس:

1. استيراد جميع الوحدات المطلوبة
2. إنشاء `ApplicationContext` (الإعدادات، المسارات، الأدوات)
3. تطبيق نمط النشر المطلوب
4. التحقق من صحة قالب DOCX
5. تنظيف المخرجات السابقة إذا تم التكوين
6. تشغيل المعالجات الأولية (Mermaid, C#, الأغلفة)
7. توليد PDF عبر Pandoc + LuaLaTeX
8. توليد DOCX عبر Pandoc + معالجة بايثون بعد الإنتاج
9. تنظيف مساحة العمل المؤقتة

المعاملات: `-SourceFile`، `-StyleProfile`، `-PdfEngine`،
`-RebuildMermaid`، `-RebuildCovers`.

### Initialize-ExportEnvironment.ps1

يدير شجرة دليل `exports/`. ينشئ ملفات placeholder في المجلدات الفارغة
(لإبقائها في Git) ويزيلها عندما تظهر ملفات المخرجات الحقيقية. يدعم
وضعين:

- **إعادة تعيين كاملة** (`-Force`): حذف جميع ملفات المخرجات، الاحتفاظ
  بـ placeholders.
- **تنظيف Placeholders** (`-CleanPlaceholdersOnly`): إزالة `placeholder.txt`
  فقط من المجلدات التي تحتوي على ملفات حقيقية.

### generate-docx-style.py (لكل نمط)

سكريبت بايثون ينشئ قالب مرجع DOCX بتنسيق احترافي باستخدام `python-docx`.
لكل نمط سكريبت خاص به، يحدد:

- عائلات الخطوط للمتن والعناوين والأكواد والتسميات التوضيحية
- أحجام الخطوط والألوان
- تباعد الفقرات والمسافات البادئة
- أنماط ترقيم العناوين
- ألوان خلفية كتل الأكواد

المخرج: `scripts/export/custom-reference.docx`

### generate-pdf-style.py (لكل نمط)

سكريبت بايثون يولد ملف سمة PDF خاص بالنمط (LaTeX) من قالب أساسي. يخصص:

- عائلات الخطوط عبر `fontspec`
- هندسة الصفحة عبر حزمة `geometry`
- أنماط العناوين
- تنسيق كتل الأكواد
- تخطيط جدول المحتويات
- تصميم الرأس والتذييل

المخرج: ملف `pdf-theme.tex` في دليل العمل.

### pdf-theme.tex

قالب LaTeX الأساسي لمخرجات PDF. هو رأس يُضمّن بواسطة Pandoc يضبط:

- فئة المستند وهندسة الصفحة
- اختيار الخطوط باستخدام `fontspec` لدعم OpenType
- إعدادات Polyglossia للغة
- تنسيق كتل الأكواد باستخدام `tcolorbox` أو `minted`
- تنسيق جدول المحتويات
- قواعد الرأس والتذييل
- إعدادات الروابط التشعبية

لكل نمط نشر ملف `pdf-theme.tex` خاص به يوسع أو يتجاوز القالب الأساسي.

### arabic-setup.tex

رأس LaTeX يُضمّن عند تصدير مستندات عربية. يضبط:

- `polyglossia` مع العربية كلغة رئيسية
- اتجاه النص من اليمين إلى اليسار
- عائلات الخطوط العربية (`Traditional Arabic`)
- تعديلات Bidi للقوائم والجداول وكتل الأكواد
- توطين التواريخ والأرقام

### codeblock-bidi.lua

مرشح Pandoc Lua ينسق كتل الأكواد لمعالجة النص ثنائي الاتجاه عبر صيغ
المخرجات المختلفة:

- **PDF/LaTeX**: يلف كتل الأكواد بـ
  `\begin{flushleft}\textdir TLT...` لإجبار العرض من اليسار لليمين
  داخل مستندات RTL.
- **DOCX**: يضع السمة `direction="ltr"` على كتل الأكواد.
- **HTML**: يضع السمة `direction="ltr"` على كتل الأكواد.

كما يوسم الكتل التي تحتوي على أحرف عربية بـ
`contains-arabic="true"` للمعالجة اللاحقة.

### escape-backslash.lua

مرشح Pandoc Lua لمخرجات LaTeX/PDF يفلت الخطوط المائلة العكسية في كتل
الأكواد إلى `\textbackslash{}`. يمنع Pandoc من تفسير أكواد C# مثل
`namespace Foo.Bar` كأوامر LaTeX.
