"""
SPDX-License-Identifier: MIT
Copyright (c) 2026 Tarek Najem
"""
import argparse, os, sys, zipfile
from lxml import etree

sys.path.insert(0, os.path.join(os.path.dirname(__file__), 'shared'))
import log

NSMAP = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}

def qn(tag):
    p, _, l = tag.partition(':')
    return f'{{{NSMAP[p]}}}{l}'

MONOKAI = {
    'NormalTok':'cfcfc2','KeywordTok':'f92672','ControlFlowTok':'f92672',
    'DataTypeTok':'a6e22e','DecValTok':'ae81ff','BaseNTok':'ae81ff',
    'FloatTok':'ae81ff','ConstantTok':'ae81ff','CharTok':'e6db74',
    'SpecialCharTok':'e6db74','StringTok':'e6db74','VerbatimStringTok':'e6db74',
    'SpecialStringTok':'e6db74','ImportTok':'a6e22e','CommentTok':'88846f',
    'DocumentationTok':'88846f','AnnotationTok':'88846f','CommentVarTok':'88846f',
    'OtherTok':'a6e22e','FunctionTok':'a6e22e','VariableTok':'fd971f',
    'OperatorTok':'f92672','BuiltInTok':'a6e22e','ExtensionTok':'a6e22e',
    'PreprocessorTok':'fd971f','AttributeTok':'fd971f','RegionMarkerTok':'75715e',
    'InformationTok':'88846f','WarningTok':'f44747','AlertTok':'f44747','ErrorTok':'f44747',
}

def apply_hl_colors(docx_path, profile_name):
    if not os.path.isfile(docx_path):
        log.log_error(f"DOCX not found: {log.rel(docx_path)}")
        sys.exit(1)

    is_dark = (profile_name == 'oreilly')
    palette = MONOKAI if is_dark else {}

    with zipfile.ZipFile(docx_path, 'r') as z:
        all_files = {info.filename: z.read(info.filename) for info in z.infolist()}

    raw = all_files.get('word/styles.xml')
    if raw is None:
        log.log_error("word/styles.xml not found in DOCX")
        sys.exit(1)

    root = etree.fromstring(raw)
    changed = False

    for style in root.findall('w:style', NSMAP):
        sid = style.get(qn('w:styleId'))
        if not sid or not sid.endswith('Tok'):
            continue
        rPr = style.find('w:rPr', NSMAP)
        if rPr is None:
            continue

        shd = rPr.find('w:shd', NSMAP)
        if shd is not None:
            rPr.remove(shd)
            changed = True

        hl = rPr.find('w:highlight', NSMAP)
        if hl is not None:
            rPr.remove(hl)
            changed = True

        if sid in palette:
            ce = rPr.find('w:color', NSMAP)
            if ce is None:
                ce = etree.SubElement(rPr, qn('w:color'))
            ce.set(qn('w:val'), palette[sid])
            changed = True

    if changed:
        all_files['word/styles.xml'] = etree.tostring(
            root, xml_declaration=True, encoding='UTF-8', standalone=True)
        with zipfile.ZipFile(docx_path, 'w', zipfile.ZIP_DEFLATED) as z:
            for name, data in all_files.items():
                z.writestr(name, data)
        log.log_info(f"  [OK] Highlight colours normalised for '{profile_name}'")
    else:
        log.log_info(f"  [SKIP] No token styles to modify")

if __name__ == "__main__":
    import argparse
    parser = argparse.ArgumentParser()
    parser.add_argument("docx_path")
    parser.add_argument("--profile", default="default")
    parser.add_argument("--config")
    parser.add_argument("--repo-root", default="")
    args = parser.parse_args()
    log.init(config_path=args.config, repo_root=args.repo_root or '')
    apply_hl_colors(args.docx_path, args.profile)
