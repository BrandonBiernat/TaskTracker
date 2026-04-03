CREATE OR REPLACE FUNCTION delete_refresh_tokens_by_user(p_user_uid UUID)
  RETURNS VOID AS $$
  BEGIN
      DELETE FROM refresh_tokens WHERE user_uid = p_user_uid;
  END;
  $$ LANGUAGE plpgsql;