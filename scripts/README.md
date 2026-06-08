# Local development scripts

This directory contains local-only scripts. CI scripts live in
`.github/workflows/`.

## Conventions

- Python: `#!/usr/bin/env python3`, run with `uv run script.py` or
  `python3 script.py`
- Bash: `#!/usr/bin/bash`, run with `bash script.sh`
- One-off scripts: `one-off/<descriptive-name>.sh` or `.py`
