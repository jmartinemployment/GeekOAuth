#!/usr/bin/env bash
# Generates a self-signed PFX for OpenIddict token signing (initial deploy / rotation).
# Do NOT commit generated files. Set output on Railway as SIGNING_CERT_BASE64.
set -euo pipefail

OUT_DIR="$(mktemp -d)"
trap 'rm -rf "$OUT_DIR"' EXIT

CN="${1:-auth.geekatyourspot.com}"
PFX_PATH="$OUT_DIR/signing.pfx"

openssl req -x509 -newkey rsa:4096 \
  -keyout "$OUT_DIR/signing.key" \
  -out "$OUT_DIR/signing.crt" \
  -days 825 -nodes \
  -subj "/CN=$CN/O=Geek OAuth/C=US"

openssl pkcs12 -export \
  -out "$PFX_PATH" \
  -inkey "$OUT_DIR/signing.key" \
  -in "$OUT_DIR/signing.crt" \
  -passout pass:

echo "Base64 (set as Railway variable SIGNING_CERT_BASE64):"
base64 < "$PFX_PATH" | tr -d '\n'
echo ""
echo ""
echo "Optional: SIGNING_CERT_PASSWORD can remain empty for this cert."
