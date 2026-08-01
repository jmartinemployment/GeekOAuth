# GeekOAuth

Standalone OAuth 2.1 / OpenID Connect authorization server (.NET 10, OpenIddict 7, Dapper Identity).

## Solution

| Project | Role |
|---------|------|
| `GeekOAuth.Abstractions` | Contracts, `ApplicationUser`, interfaces |
| `GeekOAuth.Application` | Auth logic, audit, account services |
| `GeekOAuth.Infrastructure` | Dapper repositories, Identity store |
| `GeekOAuth.Server` | Host — OpenIddict, Razor UI, EF proto/l tables |

## Local run

```bash
dotnet build GeekOAuth.slnx
export DATABASE_URL="Host=localhost;Database=geek_oauth;Username=postgres;Password=postgres"
export ISSUER_URL="https://localhost:5001"
export CORS_ORIGINS="https://localhost:4200"
export CLIENT_SECRET_GEEKAPI="dev-geekapi-secret-min-32-chars!!"
export CLIENT_SECRET_GEEKWEBSITE="dev-website-secret-min-32-chars!!"

psql "$DATABASE_URL" -f src/GeekOAuth.Server/Data/identity_tables.sql
dotnet ef database update --project src/GeekOAuth.Server --context OpenIddictDbContext
dotnet run --project src/GeekOAuth.Server
```

In Development, email links are written to the console (no SMTP required).

## Railway deploy

Set **Root Directory** to repo root. **Start command:**

```bash
dotnet run --project src/GeekOAuth.Server --configuration Release
```

Required variables: `DATABASE_URL`, `ISSUER_URL`, `SIGNING_CERT_BASE64`, `CORS_ORIGINS`, `CLIENT_SECRET_GEEKAPI`, `CLIENT_SECRET_GEEKWEBSITE`, `DATA_PROTECTION_PURPOSE`.

Optional SMTP: `SMTP_HOST`, `SMTP_PORT`, `SMTP_USER`, `SMTP_PASSWORD`, `SMTP_FROM`.

After first deploy:

```bash
psql $DATABASE_URL -f src/GeekOAuth.Server/Data/identity_tables.sql
```

## Endpoints

| Path | Purpose |
|------|---------|
| `/.well-known/openid-configuration` | OIDC discovery |
| `/.well-known/jwks.json` | Signing keys |
| `/connect/authorize` | Authorization code + PKCE |
| `/connect/token` | Tokens |
| `/connect/userinfo` | User claims |
| `/connect/revoke` | Token revocation (OpenIddict) |
| `/connect/logout` | End session |
| `/Account/Register` | Create account |
| `/Account/ConfirmEmail` | Confirm email |
| `/health` | Health check |

## Tests

```bash
dotnet test GeekOAuth.slnx
```

- **Docker required** for full integration suite (Testcontainers PostgreSQL).
- Includes **PKCE authorization-code E2E** and **client_credentials** tests when Docker is running.
- Without Docker, structural tests still pass; Docker tests are skipped.

## First-time database setup

```bash
export DATABASE_URL="Host=localhost;Database=geek_oauth;Username=postgres;Password=postgres"
./scripts/dev-setup.sh
```

Copy `.env.example` to Railway variables or your shell for production deploy.
