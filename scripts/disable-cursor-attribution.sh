#!/usr/bin/env bash
# Disable Cursor commit/PR attribution (CLI + IDE settings file when present).
set -euo pipefail

CLI_CONFIG="${HOME}/.cursor/cli-config.json"
IDE_SETTINGS="${HOME}/Library/Application Support/Cursor/User/settings.json"

python3 << 'PY'
import json
from pathlib import Path

cli = Path.home() / ".cursor/cli-config.json"
data = json.loads(cli.read_text()) if cli.exists() else {}
data.setdefault("attribution", {})
data["attribution"]["attributeCommitsToAgent"] = False
data["attribution"]["attributePRsToAgent"] = False
cli.write_text(json.dumps(data, indent=2) + "\n")
print(f"Updated {cli}")
PY

mkdir -p "$(dirname "$IDE_SETTINGS")"
if [[ -f "$IDE_SETTINGS" ]]; then
  python3 << PY
import json
from pathlib import Path
p = Path("$IDE_SETTINGS")
raw = p.read_text()
# Preserve JSONC comments loosely: only update if valid JSON
try:
    data = json.loads(raw)
except json.JSONDecodeError:
    print("IDE settings exist but contain comments; add attribution toggles manually in Cursor Settings → Agent → Attribution")
    raise SystemExit(0)
# Best-effort keys (Cursor may also use UI-only storage)
for key in (
    "cursor.commitAttribution",
    "cursor.prAttribution",
    "cursor.agent.commitAttribution",
    "cursor.agent.prAttribution",
):
    data[key] = False
p.write_text(json.dumps(data, indent=2) + "\n")
print(f"Updated {p}")
PY
else
  cat > "$IDE_SETTINGS" << 'JSON'
{
  "cursor.commitAttribution": false,
  "cursor.prAttribution": false
}
JSON
  echo "Created $IDE_SETTINGS (install/open Cursor IDE and restart once)"
fi

echo ""
echo "Also turn off in Cursor IDE (required):"
echo "  Cursor Settings → Agent → Attribution"
echo "  - Commit Attribution: OFF"
echo "  - PR Attribution: OFF"
echo "Then fully quit and reopen Cursor."
