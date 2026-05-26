# Deploy GeekOAuth to Railway

Production project: **GeekOAuth** (workspace: GeekBackend)  
Public URL: **https://auth.geekatyourspot.com**

## One-time setup

### 0. GitHub Actions workflows (local only until pushed)

Workflow files live in `.github/workflows/` but require the `workflow` OAuth scope to push.
From your machine:

```bash
gh auth refresh -h github.com -s workflow
git add .github/workflows/
git commit -m "ci: add GitHub Actions build and Railway deploy workflows"
git push origin main
```

Add repo secret `RAILWAY_TOKEN` before the deploy workflow can run.

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

## Custom domain troubleshooting (Namecheap)

The app listens on port **8080** inside Railway (set in Railway Networking, not in DNS).

If `https://geekoauth-production.up.railway.app/health` is **Healthy** but
`https://auth.geekatyourspot.com/health` returns `404 Application not found`, the
CNAME target Railway gave you (`*.up.railway.app`) may be orphaned. **Fix in Namecheap:**

| Type | Host | Value |
|------|------|--------|
| **CNAME** | `auth` | **`geekoauth-production.up.railway.app`** |

Do **not** use a stale hostname like `mlrz6mwl.up.railway.app` if that host also returns 404.

Verify:

```bash
dig +short auth.geekatyourspot.com CNAME
curl -sS https://auth.geekatyourspot.com/health
```

Alternatively, remove and re-add the domain in Railway → GeekOAuth → Networking (port **8080**)
and use the **new** CNAME Railway prints.

## Verify

```bash
curl -sS https://geekoauth-production.up.railway.app/health
curl -sS https://auth.geekatyourspot.com/health
curl -sS https://auth.geekatyourspot.com/.well-known/openid-configuration | jq .issuer
```

Expected issuer: `https://auth.geekatyourspot.com`

## CI

Every push/PR runs **CI** workflow: `dotnet build` + full test suite (Docker + PKCE E2E on `ubuntu-latest`).
