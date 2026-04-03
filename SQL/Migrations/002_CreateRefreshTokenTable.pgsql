CREATE TABLE refresh_tokens (
    uid UUID PRIMARY KEY,
    user_uid UUID NOT NULL,
    token_hash TEXT NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_refresh_token_user_uid ON refresh_tokens (user_uid);