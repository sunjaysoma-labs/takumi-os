#!/usr/bin/env python3
"""
Verify that every OpenAPI spec in docs/api/openapi/ is consistent
with the Azure Functions / ASP.NET controllers / agents in src/.

Currently a stub — full implementation is post-M0.
"""
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[1]
OPENAPI_DIR = REPO_ROOT / "docs" / "api" / "openapi"


def main():
    if not OPENAPI_DIR.exists():
        print(f"OpenAPI directory not found (expected M1+ work): {OPENAPI_DIR}")
        return 0
    specs = list(OPENAPI_DIR.glob("*.yaml")) + list(OPENAPI_DIR.glob("*.yml"))
    print(f"Found {len(specs)} OpenAPI specs. (Consistency check is M1+ work.)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
