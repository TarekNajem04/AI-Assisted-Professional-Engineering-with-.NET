"""
SPDX-License-Identifier: MIT
Copyright (c) 2025 Tarek Najem

This file is part of the AI-Assisted Professional Engineering with .NET
book project (https://github.com/TarekNajem/AI-Assisted-Professional-Engineering-With-.NET).
It is subject to the terms and conditions of the MIT License as published
in the LICENSE file at the root of this repository.

This header must not be removed or modified without preserving the
LICENSE file reference and copyright notice.

Description:
    Post-processes a DOCX file to ensure all images fit within the page
    content area. Reads page dimensions and margins from the DOCX's
    section properties, then scales each inline image proportionally to
    avoid overflow. Requires python-docx.
"""

import sys, os, json
from docx import Document
from docx.shared import Emu, Cm, Mm


def emu_to_cm(emu):
    return emu / 360000.0


def cm_to_emu(cm):
    return int(cm * 360000)


def fit_images(docx_path, caption_reserve_cm=1.4):
    doc = Document(docx_path)
    section = doc.sections[0]

    pw_cm = emu_to_cm(section.page_width)
    ph_cm = emu_to_cm(section.page_height)
    mt_cm = emu_to_cm(section.top_margin)
    mb_cm = emu_to_cm(section.bottom_margin)
    ml_cm = emu_to_cm(section.left_margin)
    mr_cm = emu_to_cm(section.right_margin)

    avail_w_cm = pw_cm - ml_cm - mr_cm
    avail_h_cm = ph_cm - mt_cm - mb_cm - caption_reserve_cm

    fixed = 0
    skipped = 0

    for shape in doc.inline_shapes:
        w_emu = shape.width
        h_emu = shape.height

        if w_emu <= 0 or h_emu <= 0:
            skipped += 1
            continue

        w_cm = emu_to_cm(w_emu)
        h_cm = emu_to_cm(h_emu)
        aspect = w_cm / h_cm
        avail_aspect = avail_w_cm / avail_h_cm

        if w_cm <= avail_w_cm and h_cm <= avail_h_cm:
            skipped += 1
            continue

        if aspect >= avail_aspect:
            new_w_cm = avail_w_cm
            new_h_cm = avail_w_cm / aspect
        else:
            new_h_cm = avail_h_cm
            new_w_cm = avail_h_cm * aspect

        shape.width = cm_to_emu(new_w_cm)
        shape.height = cm_to_emu(new_h_cm)
        fixed += 1

    doc.save(docx_path)

    result = {
        "fixed": fixed,
        "skipped": skipped,
        "avail_w_cm": round(avail_w_cm, 1),
        "avail_h_cm": round(avail_h_cm, 1)
    }
    print(json.dumps(result))
    return fixed > 0


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print(json.dumps({"error": "Usage: python docx-fit-images.py <docx_path> [--caption-reserve-cm X]"}))
        sys.exit(1)

    docx_path = sys.argv[1]
    caption_reserve = 1.4
    if "--caption-reserve-cm" in sys.argv:
        idx = sys.argv.index("--caption-reserve-cm")
        if idx + 1 < len(sys.argv):
            caption_reserve = float(sys.argv[idx + 1])

    if not os.path.exists(docx_path):
        print(json.dumps({"error": f"File not found: {docx_path}"}))
        sys.exit(1)

    try:
        fit_images(docx_path, caption_reserve)
    except Exception as e:
        print(json.dumps({"error": str(e)}))
        sys.exit(1)
