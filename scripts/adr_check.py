#!/usr/bin/env python3
"""
Verify that every ADR in docs/adr/ is referenced from at least one
code/config file in the repo. Stale ADRs (accepted but never
referenced) are a sign of drift.
"""
import os
import re
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[1]
ADR_DIR = REPO_ROOT / "docs" / "adr"

# Patterns that count as "referenced"
REFERENCE_PATTERNS = [
    re.compile(r"000\d-[\w-]+"),
]

EXCLUDE_DIRS = {
    ".git", "node_modules", "dist", "build", ".next", ".venv", "venv",
    "TestResults", "coverage", "coveragereport", ".terraform", ".ruff_cache",
    ".mypy_cache", ".pytest_cache", "__pycache__", "playwright-report",
}


def find_adr_files():
    adrs = []
    for path in ADR_DIR.glob("*.md"):
        adrs.append(path)
    return adrs


def adr_slug(path: Path) -> str:
    return path.stem  # e.g. "0001-reasoning-model-minimax"


def is_referenced(slug: str) -> bool:
    pattern = re.compile(re.escape(slug))
    for root, dirs, files in os.walk(REPO_ROOT):
        dirs[:] = [d for d in dirs if d not in EXCLUDE_DIRS and not d.startswith(".")]
        for f in files:
            if f.startswith("."):
                continue
            p = Path(root) / f
            # Skip the ADR file itself and the ADR index
            if ADR_DIR in p.parents:
                continue
            try:
                if pattern.search(p.read_text(errors="ignore")):
                    return True
            except (OSError, UnicodeDecodeError):
                continue
    return False


def main():
    if not ADR_DIR.exists():
        print(f"ADR directory not found: {ADR_DIR}")
        return 0

    adrs = find_adr_files()
    if not adrs:
        print("No ADRs found.")
        return 0

    unreferenced = []
    for adr in sorted(adrs):
        slug = adr_slug(adr)
        if not is_referenced(slug):
            unreferenced.append(slug)

    if unreferenced:
        print("The following ADRs are not referenced from any code/config:")
        for slug in unreferenced:
            print(f"  - {slug}")
        print()
        print("Either:")
        print("  (a) reference them in a code or config file, or")
        print("  (b) if the decision is no longer relevant, archive them per DIP §3.")
        return 1

    print(f"All {len(adrs)} ADRs are referenced. Wiki ↔ repo OK.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
