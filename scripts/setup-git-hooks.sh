#!/usr/bin/env bash
# Install repo hooks so AI Co-authored-by trailers never land in commits.
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
HOOKS="$ROOT/.githooks"
git -C "$ROOT" config core.hooksPath "$HOOKS"
echo "Installed git hooks from $HOOKS"
