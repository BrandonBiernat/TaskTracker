CREATE OR REPLACE FUNCTION get_refresh_token(p_token_hash TEXT)
  RETURNS TABLE (
      uid UUID,
      user_uid UUID,
      token_hash TEXT,
      expires_at TIMESTAMPTZ,
      created_at TIMESTAMPTZ
  ) AS $$
  BEGIN
      RETURN QUERY SELECT rt.uid, rt.user_uid, rt.token_hash, rt.expires_at, rt.created_at
      FROM refresh_tokens rt
      WHERE rt.token_hash = p_token_hash;
  END;
  $$ LANGUAGE plpgsql;