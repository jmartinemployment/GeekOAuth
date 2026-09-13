# Forwarded-header trust design (GeekOAuth)

**Status:** Implemented in code (2026-09-13). Fill environment-specific CIDRs before production rely-on.

## Problem

`Program.cs` previously cleared `KnownIPNetworks` / `KnownProxies`, which made every client a trusted proxy. Rate limits keyed on `RemoteIpAddress` could then be bypassed by rotating `X-Forwarded-For`.

## Trust model

| Environment | Trusted proxies | Behavior if misconfigured |
|-------------|-----------------|---------------------------|
| Development | Loopback only (ASP.NET defaults) | Direct requests keep real IP; forged `X-Forwarded-For` ignored |
| Production (Railway) | CIDRs/IPs from `TRUSTED_PROXY_CIDRS` / `TRUSTED_PROXY_IPS` | If unset: **fail closed** — do not clear known lists; forwarded headers only accepted from defaults (loopback). Edge-proxied client IP may not be seen until CIDRs are set. |

## Required config (production)

```bash
# Comma-separated CIDRs for the reverse-proxy hop that terminates TLS in front of GeekOAuth
TRUSTED_PROXY_CIDRS=10.0.0.0/8,fd12:3456:789a::/48

# Optional exact proxy IPs
TRUSTED_PROXY_IPS=
```

Update procedure when Railway/ingress IP ranges change: revise these env vars and redeploy GeekOAuth; no code change.

## Tests to run after deploy

1. **Direct public request** with forged `X-Forwarded-For: 1.2.3.4` — rate-limit partition must use the real connecting IP, not `1.2.3.4`.
2. **Ingress-proxied request** from a trusted CIDR — partition uses the forwarded client IP.

## Related

- Rate limiting: `AddGeekRateLimiting()` in GeekOAuth.Server
- Code: `ForwardedHeadersConfiguration.Apply` in `Program.cs`
