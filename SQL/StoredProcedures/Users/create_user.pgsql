CREATE OR REPLACE FUNCTION create_user(                                                                                                     
      p_uid UUID,                        
      p_email VARCHAR(256),                                                                                                                   
      p_password_hash TEXT,
      p_first_name VARCHAR(100),
      p_last_name VARCHAR(100)                                                                                                                
  ) RETURNS VOID AS $$        
  BEGIN                                                                                                                                       
      INSERT INTO users (uid, email, password_hash, first_name, last_name)
      VALUES (p_uid, p_email, p_password_hash, p_first_name, p_last_name);
  END;                                                                                                                                        
  $$ LANGUAGE plpgsql;