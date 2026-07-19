"""
SPDX-License-Identifier: MIT
Copyright (c) 2025 Tarek Najem

This file is part of the AI-Assisted Professional Engineering with .NET
book project (https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET).
It is subject to the terms and conditions of the MIT License as published
in the LICENSE file at the root of this repository.

This header must not be removed or modified without preserving the
LICENSE file reference and copyright notice.

Description:
    Verify and set page margins on a Pandoc-generated DOCX file.
    Runs after Pandoc and before cover insertion.
"""
import argparse, json, os, sys
from docx import Document
from docx.shared import Cm
from docx.oxml.ns import qn
from docx.oxml import OxmlElement

sys.path.insert(0, os.path.join(os.path.dirname(__file__), 'shared'))
import log


_TWIPS_PER_CM = 567  # 1440 twips/inch / 2.54 cm/inch


def set_margins(docx_path, config_path=None):
    if not os.path.isfile(docx_path):
        log.log_error(f"DOCX not found: {log.rel(docx_path)}")
        sys.exit(1)

    margins = {'top': 2.0, 'bottom': 2.5, 'left': 2.8, 'right': 1.8}
    if config_path and os.path.isfile(config_path):
        with open(config_path, 'r', encoding='utf-8') as f:
            cfg = json.load(f)
        m = cfg.get('docx', {}).get('margins', {})
        margins = {
            'top':    m.get('top', 2.0),
            'bottom': m.get('bottom', 2.5),
            'left':   m.get('left', 2.8),
            'right':  m.get('right', 1.8),
        }

    doc = Document(docx_path)
    # Normalize: extract embedded sectPr if pandoc put it inside a paragraph
    body = doc.element.body
    last_child = list(body)[-1] if len(body) > 0 else None
    if last_child is not None:
        nested = last_child.find(qn('w:pPr') + '/' + qn('w:sectPr'))
        if nested is not None:
            pPr = nested.getparent()
            pPr.remove(nested)
            body.append(nested)
            log.log_info("  [Normalised] sectPr extracted from paragraph to direct body child")
    for section in doc.sections:
        sectPr = section._sectPr
        pgMar = sectPr.find(qn('w:pgMar'))
        if pgMar is None:
            pgMar = OxmlElement('w:pgMar')
            sectPr.insert(0, pgMar)

        t_twip = str(int(margins['top'] * _TWIPS_PER_CM + 0.5))
        b_twip = str(int(margins['bottom'] * _TWIPS_PER_CM + 0.5))
        l_twip = str(int(margins['left'] * _TWIPS_PER_CM + 0.5))
        r_twip = str(int(margins['right'] * _TWIPS_PER_CM + 0.5))

        pgMar.set(qn('w:top'),    t_twip)
        pgMar.set(qn('w:bottom'), b_twip)
        pgMar.set(qn('w:left'),   l_twip)
        pgMar.set(qn('w:right'),  r_twip)

    doc.save(docx_path)

    # Verify by re-reading
    doc2 = Document(docx_path)
    for i, s in enumerate(doc2.sections):
        actual_t = s.top_margin / 360000
        actual_b = s.bottom_margin / 360000
        actual_l = s.left_margin / 360000
        actual_r = s.right_margin / 360000
        log.log_info(f"  Verify Section {i}: T={actual_t:.2f} B={actual_b:.2f} L={actual_l:.2f} R={actual_r:.2f} cm")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Set DOCX page margins")
    parser.add_argument("docx_path", help="Path to the DOCX file")
    parser.add_argument("--config", help="Path to export-config.json")
    parser.add_argument("--repo-root", default="", help="Repository root for relative paths")
    args = parser.parse_args()
    log.init(config_path=args.config, repo_root=args.repo_root or '')
    set_margins(args.docx_path, args.config)

