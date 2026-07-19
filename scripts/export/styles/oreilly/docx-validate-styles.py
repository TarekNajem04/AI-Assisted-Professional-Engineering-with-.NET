"""
SPDX-License-Identifier: MIT
Copyright (c) 2026 Tarek Najem

This file is part of the AI-Assisted Professional Engineering with .NET
book project (https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET).
It is subject to the terms and conditions of the MIT License as published
in the LICENSE file at the root of this repository.

This header must not be removed or modified without preserving the
LICENSE file reference and copyright notice.

Description:
    Validate that the reference DOCX template contains all required styles
    for the active profile. Exits with code 0 on success, 1 on failure.
"""

import sys
import os
import json


def _rel(path, root):
    if not root:
        return os.path.basename(path)
    p = path.replace('\\', '/')
    r = root.replace('\\', '/')
    if p.startswith(r):
        return p[len(r):].lstrip('/')
    return os.path.basename(path)

try:
    from docx import Document
    from docx.shared import Pt
except ImportError:
    print("ERROR: python-docx is not installed.")
    print("Install with: pip install python-docx")
    sys.exit(1)

# Per-profile style expectations: styles that MUST exist with exact font/size
STYLE_PROFILES = {'oreilly': {'paragraph': {'BookBody': {'font': 'Segoe UI', 'size': 10.5}, 'BookHeading1': {'font': 'Open Sans', 'size': 24}, 'BookHeading2': {'font': 'Open Sans', 'size': 16}, 'BookHeading3': {'font': 'Open Sans', 'size': 13}, 'BookCode': {'font': 'Consolas', 'size': 9}, 'BookQuote': {'font': 'Segoe UI', 'size': 10.5}, 'BookTable': {'font': 'Segoe UI', 'size': 9}, 'BookCaption': {'font': 'Open Sans', 'size': 9}, 'BookImageCaption': {'font': 'Open Sans', 'size': 9}, 'BookDate': {'font': 'Segoe UI', 'size': 10.5}, 'BookTOC1': {'font': 'Open Sans', 'size': 12}, 'BookTOC2': {'font': 'Open Sans', 'size': 11}, 'BookTOC3': {'font': 'Open Sans', 'size': 10}}, 'character': {'BookInlineCode': {'font': 'Consolas', 'size': 9}}}}


PANDOC_CORE_STYLES = {
    "Normal":     {"type": "paragraph"},
    "Body Text":  {"type": "paragraph"},
    "Heading 1":  {"type": "paragraph"},
    "Heading 2":  {"type": "paragraph"},
    "Heading 3":  {"type": "paragraph"},
    "Source Code":{"type": "paragraph"},
    "Title":      {"type": "paragraph"},
    "Subtitle":   {"type": "paragraph"},
    "Caption":    {"type": "paragraph"},
}


def check_style_matches(style, expected, name):
    """Check if a style matches expected font and size. Returns (ok, messages)."""
    font = style.font
    ok = True
    details = []
    if font.name != expected["font"]:
        details.append(f"font={font.name} (expected {expected['font']})")
        ok = False
    expected_size = Pt(expected["size"])
    if font.size != expected_size:
        details.append(f"size={font.size} (expected {expected_size})")
        ok = False
    return ok, details


def validate_styles(docx_path, profile_name="default", project_root=None):
    if not os.path.exists(docx_path):
        print(f"File not found: {_rel(docx_path, project_root)}")
        return False

    profile = STYLE_PROFILES.get(profile_name, list(STYLE_PROFILES.values())[0])
    doc = Document(docx_path)
    style_names = [s.name for s in doc.styles]
    all_ok = True

    print(f"Validating: {_rel(docx_path, project_root)}")
    print(f"Profile    : {profile_name}")
    print()

    # ── Required paragraph styles ──
    print("--- Paragraph Styles ---")
    for name, expected in profile["paragraph"].items():
        if name not in style_names:
            print(f"  MISSING  {name}")
            all_ok = False
            continue

        style = doc.styles[name]
        ok, details = check_style_matches(style, expected, name)
        mark = "+" if ok else "x"
        detail_str = f" \u2014 {'; '.join(details)}" if details else ""
        print(f"  [{mark}] {name}: {style.font.name}, {style.font.size}{detail_str}")
        if not ok:
            all_ok = False

    # ── Required character styles ──
    print()
    print("--- Character Styles ---")
    for name, expected in profile["character"].items():
        if name not in style_names:
            print(f"  MISSING  {name}")
            all_ok = False
            continue

        style = doc.styles[name]
        ok, details = check_style_matches(style, expected, name)
        mark = "+" if ok else "x"
        detail_str = f" \u2014 {'; '.join(details)}" if details else ""
        print(f"  [{mark}] {name}: {style.font.name}, {style.font.size}{detail_str}")
        if not ok:
            all_ok = False

    # ── Warn about missing Pandoc-default styles ──
    print()
    missing_pandoc = []
    for name, _ in PANDOC_CORE_STYLES.items():
        if name not in style_names:
            missing_pandoc.append(name)
    if missing_pandoc:
        print(f"Note: Pandoc-default styles not in template: {', '.join(missing_pandoc)}")
        print("      (Pandoc creates them on output; optional in reference docx)")

    print()
    if all_ok:
        print("RESULT: All styles validated successfully.")
    else:
        print("RESULT: One or more styles are missing or incorrect.")
    return all_ok


if __name__ == "__main__":
    import argparse
    parser = argparse.ArgumentParser()
    parser.add_argument('--reference-doc', default=None)
    parser.add_argument('--profile', default="oreilly",
                        help="Style profile name (default, apress, ...)")
    parser.add_argument('--project-root', default=None)
    args = parser.parse_args()
    if args.reference_doc:
        docx_path = args.reference_doc
    else:
        script_dir = os.path.dirname(os.path.abspath(__file__))
        docx_path = os.path.join(script_dir, "custom-reference.docx")
    success = validate_styles(docx_path, args.profile, args.project_root)
    sys.exit(0 if success else 1)
