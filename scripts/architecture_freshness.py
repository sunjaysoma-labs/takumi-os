#!/usr/bin/env python3
"""
Flag architecture docs that haven't been touched in 90+ days but are
referenced by recent commits — a sign that code has moved on but
docs haven't.
"""
import datetime as dt
import re
import subprocess
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[1]
ARCH_DIR = REPO_ROOT / "docs" / "architecture"
FRESH_DAYS = 90


def last_modified(path: Path) -> dt.datetime:
    return dt.datetime.fromtimestamp(path.stat().st_mtime)


def referenced_in_recent_commits(path: Path) -> bool:
    rel = path.relative_to(REPO_ROOT)
    try:
        out = subprocess.run(
            ["git", "log", "--since=30 days ago", "--name-only", "--pretty=format:"],
            cwd=REPO_ROOT, capture_output=True, text=True, check=True,
        ).stdout
    except subprocess.CalledProcessError:
        return False
    return str(rel) in out


def main():
    if not ARCH_DIR.exists():
        return 0
    cutoff = dt.datetime.now() - dt.timedelta(days=FRESH_DAYS)
    stale = []
    for path in ARCH_DIR.glob("*.md"):
        mtime = last_modified(path)
        if mtime < cutoff and referenced_in_recent_commits(path):
            stale.append((path, mtime))
    if stale:
        print("Stale architecture docs referenced by recent commits:")
        for path, mtime in stale:
            days = (dt.datetime.now() - mtime).days
            print(f"  - {path.relative_to(REPO_ROOT)} (last touched {days} days ago)")
        return 1
    print("Architecture docs are fresh (within 90 days).")
    return 0


if __name__ == "__main__":
    sys.exit(main())
