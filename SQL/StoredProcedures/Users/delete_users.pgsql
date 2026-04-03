CREATE OR REPLACE FUNCTION delete_users(uid_list UUID[])
  RETURNS VOID AS $$                                                                                                                          
  BEGIN             
      DELETE FROM users WHERE uid = ANY(uid_list);                                                                                            
  END;                                            
  $$ LANGUAGE plpgsql;