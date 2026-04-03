CREATE OR REPLACE FUNCTION update_user(                                                                                                     
      p_uid UUID,                        
      p_password_hash TEXT,                                                                                                                   
      p_first_name VARCHAR(100),
      p_last_name VARCHAR(100)  
  ) RETURNS VOID AS $$        
  BEGIN               
      UPDATE users                                                                                                                            
      SET password_hash = p_password_hash,
          first_name = p_first_name,                                                                                                          
          last_name = p_last_name   
      WHERE uid = p_uid;         
  END;                  
  $$ LANGUAGE plpgsql;