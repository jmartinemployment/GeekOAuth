-- Identity tables owned by Dapper — run once against the auth server DB

CREATE TABLE IF NOT EXISTS asp_net_users (
    id                    TEXT        NOT NULL PRIMARY KEY,
    user_name             TEXT,
    normalized_user_name  TEXT        UNIQUE,
    email                 TEXT,
    normalized_email      TEXT,
    email_confirmed       BOOLEAN     NOT NULL DEFAULT FALSE,
    password_hash         TEXT,
    security_stamp        TEXT,
    concurrency_stamp     TEXT,
    phone_number          TEXT,
    phone_number_confirmed BOOLEAN    NOT NULL DEFAULT FALSE,
    two_factor_enabled    BOOLEAN     NOT NULL DEFAULT FALSE,
    lockout_end           TIMESTAMPTZ,
    lockout_enabled       BOOLEAN     NOT NULL DEFAULT TRUE,
    access_failed_count   INTEGER     NOT NULL DEFAULT 0,
    display_name          TEXT,
    tenant                TEXT
);

CREATE TABLE IF NOT EXISTS asp_net_roles (
    id              TEXT NOT NULL PRIMARY KEY,
    name            TEXT,
    normalized_name TEXT UNIQUE,
    concurrency_stamp TEXT
);

CREATE TABLE IF NOT EXISTS asp_net_user_roles (
    user_id TEXT NOT NULL REFERENCES asp_net_users(id) ON DELETE CASCADE,
    role_id TEXT NOT NULL REFERENCES asp_net_roles(id) ON DELETE CASCADE,
    PRIMARY KEY (user_id, role_id)
);

CREATE TABLE IF NOT EXISTS asp_net_user_claims (
    id          SERIAL PRIMARY KEY,
    user_id     TEXT NOT NULL REFERENCES asp_net_users(id) ON DELETE CASCADE,
    claim_type  TEXT,
    claim_value TEXT
);

CREATE TABLE IF NOT EXISTS asp_net_role_claims (
    id          SERIAL PRIMARY KEY,
    role_id     TEXT NOT NULL REFERENCES asp_net_roles(id) ON DELETE CASCADE,
    claim_type  TEXT,
    claim_value TEXT
);

CREATE TABLE IF NOT EXISTS asp_net_user_logins (
    login_provider        TEXT NOT NULL,
    provider_key          TEXT NOT NULL,
    provider_display_name TEXT,
    user_id               TEXT NOT NULL REFERENCES asp_net_users(id) ON DELETE CASCADE,
    PRIMARY KEY (login_provider, provider_key)
);

CREATE TABLE IF NOT EXISTS asp_net_user_tokens (
    user_id        TEXT NOT NULL REFERENCES asp_net_users(id) ON DELETE CASCADE,
    login_provider TEXT NOT NULL,
    name           TEXT NOT NULL,
    value          TEXT,
    PRIMARY KEY (user_id, login_provider, name)
);

CREATE INDEX IF NOT EXISTS idx_asp_net_users_normalized_email ON asp_net_users(normalized_email);
CREATE INDEX IF NOT EXISTS idx_asp_net_user_claims_user_id ON asp_net_user_claims(user_id);
CREATE INDEX IF NOT EXISTS idx_asp_net_user_roles_user_id ON asp_net_user_roles(user_id);
