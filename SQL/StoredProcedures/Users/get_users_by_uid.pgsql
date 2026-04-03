CREATE OR REPLACE FUNCTION get_users_by_uid(uid_list UUID[])
  RETURNS TABLE (                                                                                                                             
      uid UUID,                                                                                                                               
      email VARCHAR(256),
      password_hash TEXT,                                                                                                                     
      first_name VARCHAR(100),
      last_name VARCHAR(100), 
      created_date TIMESTAMPTZ
  ) AS $$                     
  BEGIN  
      RETURN QUERY SELECT u.uid, u.email, u.password_hash, u.first_name, u.last_name, u.created_date
      FROM users u                                                                                  
      WHERE u.uid = ANY(uid_list);                                                                                                            
  END;                            
  $$ LANGUAGE plpgsql; 