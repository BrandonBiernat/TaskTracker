CREATE OR REPLACE FUNCTION delete_refresh_token(p_uid UUID)
RETURNS VOID AS $$
BEGIN
    DELETE FROM refresh_tokens WHERE uid = p_uid;
END;
$$ LANGUAGE plpgsql;