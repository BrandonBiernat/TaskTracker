# TaskTracker

A task tracking API built with .NET 10 and PostgreSQL.

## Database Setup

### First-time setup

1. Start the Postgres container:
   ```bash
   docker run --name tasktracker-db \
     -e POSTGRES_PASSWORD=postgres \
     -e POSTGRES_USER=postgres \
     -e POSTGRES_DB=TaskTracker \
     -p 5432:5432 \
     -v tasktracker-pgdata:/var/lib/postgresql/data \
     -d postgres:16
   ```

2. Run migrations (creates tables):
   ```bash
   docker exec -i tasktracker-db psql -U postgres -d TaskTracker < SQL/Migrations/001_CreateUsersTable.pgsql
   ```

3. Deploy functions:
   ```bash
   docker exec -i tasktracker-db psql -U postgres -d TaskTracker < SQL/StoredProcedures/{table_name}}/{function_name}
   ```

4. Set the connection string:
   ```bash
   cd api
   dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=TaskTracker;Username=postgres;Password=postgres"
   ```

### Adding new migrations

Create a new file in `SQL/Migrations/` with the next number prefix (e.g., `002_create_tasks_table.pgsql`). Run it against the database:

```bash
docker exec -i tasktracker-db psql -U postgres -d TaskTracker < SQL/Migrations/002_create_tasks_table.pgsql
```

Migrations run once and should never be edited after being applied. To change a table, create a new migration with `ALTER TABLE`.

### Adding or updating functions

Create or edit a `.pgsql` file in `SQL/StoredProcedures/<table_name>/`. Functions use `CREATE OR REPLACE` so they are safe to re-run:

```bash
docker exec -i tasktracker-db psql -U postgres -d TaskTracker < SQL/StoredProcedures/<table_name>/<function_name>.pgsql
```

To verify deployed functions:

```bash
docker exec -i tasktracker-db psql -U postgres -d TaskTracker -c "\df public.*"
```

### Starting and stopping

After the first run, the volume persists your data. Just use:

```bash
docker start tasktracker-db
docker stop tasktracker-db
```

### Querying the database

```bash
docker exec -it tasktracker-db psql -U postgres -d TaskTracker
```

## Running the API

```bash
cd api
dotnet run
```
