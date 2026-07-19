"""
SPDX-License-Identifier: MIT
Copyright (c) 2026 Tarek Najem

This file is part of the AI-Assisted Professional Engineering with .NET
book project (https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET).

Description:
    Generate/adjust the PDF theme file for the default style profile.
    Copies the original pdf-theme.tex to the working directory.
"""

import shutil
import os
import sys
import argparse


def _rel(path, root):
    if not root:
        return os.path.basename(path)
    p = path.replace('\\', '/')
    r = root.replace('\\', '/')
    if p.startswith(r):
        return p[len(r):].lstrip('/')
    return os.path.basename(path)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument('--output', required=True,
                        help='Output path for generated PDF theme')
    parser.add_argument('--template', required=True,
                        help='Source pdf-theme.tex path')
    parser.add_argument('--config', default=None,
                        help='Path to export-config.json (unused in default profile)')
    parser.add_argument('--project-root', default=None)
    args = parser.parse_args()

    os.makedirs(os.path.dirname(os.path.abspath(args.output)), exist_ok=True)
    shutil.copy2(args.template, args.output)
    print(f"  [PROFILE] PDF theme copied: {_rel(args.template, args.project_root)} \u2192 {_rel(args.output, args.project_root)}")


if __name__ == "__main__":
    main()
