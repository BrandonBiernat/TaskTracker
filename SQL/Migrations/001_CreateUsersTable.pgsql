CREATE TABLE Users (
    uid UUID PRIMARY KEY,
    email VARCHAR(256) NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    created_date TIMESTAMPTZ NOT NULL DEFAULT NOW()
)