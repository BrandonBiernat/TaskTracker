CREATE OR REPLACE FUNCTION create_refresh_token(
    p_uid UUID,
    p_user_uid UUID,
    p_token_hash TEXT,
    p_expires_at TIMESTAMPTZ
) RETURNS VOID AS $$
BEGIN
    INSERT INTO refresh_tokens (uid, user_uid, token_hash, expires_at)
    VALUES (p_uid, p_user_uid, p_token_hash, p_expires_at);
END;
$$ LANGUAGE plpgsql;