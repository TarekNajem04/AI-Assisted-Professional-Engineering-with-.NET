"""
SPDX-License-Identifier: MIT
Copyright (c) 2026 Tarek Najem

This file is part of the AI-Assisted Professional Engineering with .NET
book project (https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET).
It is subject to the terms and conditions of the MIT License as published
in the LICENSE file at the root of this repository.

This header must not be removed or modified without preserving the
LICENSE file reference and copyright notice.
"""
"""
Shared logging for Python export scripts.
Emits ANSI-colored output using log.colors from export-config.json.
When piped through PowerShell Write-Log, ANSI codes pass through to the terminal.
"""
import os, json

_REPO_ROOT = ''
_ANSI = {}  # color_key -> ANSI escape code

_ANSI_MAP = {
    'Cyan': '36', 'DarkCyan': '36',
    'DarkMagenta': '35',
    'DarkYellow': '33', 'Yellow': '33',
    'DarkGreen': '32', 'Green': '32',
    'Red': '31', 'DarkRed': '31',
    'DarkGray': '90', 'Gray': '37',
    'White': '97', 'Black': '30',
    'Blue': '34', 'DarkBlue': '34',
}

def init(config_path=None, repo_root=''):
    global _REPO_ROOT, _ANSI
    _REPO_ROOT = repo_root or ''
    if config_path:
        try:
            with open(config_path, encoding='utf-8') as f:
                cfg = json.load(f)
            colors = cfg.get('log', {}).get('colors', {})
            for key, pscolor in colors.items():
                code = _ANSI_MAP.get(pscolor, '')
                if code:
                    _ANSI[key] = f'\033[{code}m'
        except Exception as e:
            import sys
            print(f"  [log.py] Failed to load config colors: {e}", file=sys.stderr)

def rel(path):
    if _REPO_ROOT and path:
        return os.path.relpath(path, _REPO_ROOT)
    return path or ''

def _c(color_key, message):
    code = _ANSI.get(color_key, '')
    reset = '\033[0m' if code else ''
    return f'{code}{message}{reset}'

def log_error(message):
    print(_c('Error', f'  ERROR: {message}'))

def log_warn(message):
    print(_c('Warning', f'  [WARN] {message}'))

def log_ok(message):
    print(_c('Success', f'  [OK] {message}'))

def log_info(message):
    print(_c('Info', f'  {message}'))

def log_muted(message):
    print(_c('Muted', f'  {message}'))
