#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

if [[ -z "${DATABASE_URL:-}" ]]; then
  echo "Set DATABASE_URL before running this script."
  exit 1
fi

echo "Applying Identity tables..."
psql "$DATABASE_URL" -f src/GeekOAuth.Server/Data/identity_tables.sql

echo "Applying EF migrations (OpenIddict + DataProtection)..."
dotnet ef database update --project src/GeekOAuth.Server --context OpenIddictDbContext

echo "Done. Start the server with:"
echo "  dotnet run --project src/GeekOAuth.Server"
