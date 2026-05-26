# Deploy GeekOAuth to Railway

Production project: **GeekOAuth** (workspace: GeekBackend)  
Public URL: **https://auth.geekatyourspot.com**

## One-time setup

### 1. GitHub secrets (for CI deploy workflow)

In the GitHub repo → Settings → Secrets → Actions, add:

| Secret | How to get it |
|--------|----------------|
| `RAILWAY_TOKEN` | Railway → Account Settings → Tokens → Create token |

### 2. Signing certificate (required in Production)

```bash
./scripts/generate-signing-cert.sh auth.geekatyourspot.com > /tmp/signing.b64
railway variables set SIGNING_CERT_BASE64="$(cat /tmp/signing.b64)" --service GeekOAuth
rm /tmp/signing.b64
```

Use a proper CA-issued cert in production when available; rotate by updating the variable and redeploying.

### 3. Identity database tables (once per Postgres instance)

```bash
railway link --project GeekOAuth --environment production --service GeekOAuth
railway run bash -c 'psql "$DATABASE_URL" -f src/GeekOAuth.Server/Data/identity_tables.sql'
```

OpenIddict + DataProtection tables are created automatically on app startup via EF migrations.

## Deploy

### From GitHub (recommended)

Push to `main` → **Deploy Railway** workflow runs (or trigger manually under Actions).

### From CLI

```bash
cd GeekOAuth
railway link --project GeekOAuth --environment production --service GeekOAuth
railway up --detach
```

## Verify

```bash
curl -sS https://auth.geekatyourspot.com/health
curl -sS https://auth.geekatyourspot.com/.well-known/openid-configuration | jq .issuer
```

Expected issuer: `https://auth.geekatyourspot.com`

## CI

Every push/PR runs **CI** workflow: `dotnet build` + full test suite (Docker + PKCE E2E on `ubuntu-latest`).
