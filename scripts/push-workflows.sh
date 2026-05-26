#!/usr/bin/env bash
# Push .github/workflows (requires GitHub token with "workflow" scope).
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

if ! gh auth status 2>&1 | grep -q workflow; then
  echo "GitHub token needs workflow scope. Run:"
  echo "  gh auth refresh -h github.com -s workflow,repo"
  echo "Complete the browser step, then re-run: $0"
  exit 1
fi

git push origin main
echo "Workflows pushed. Check: https://github.com/jmartinemployment/GeekOAuth/actions"
