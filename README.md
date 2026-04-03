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

2. Run migrations:
   ```bash
   docker exec -i tasktracker-db psql -U postgres -d TaskTracker -f - < SQL/Migrations/001_CreateUsersTable.pgsql
   ```

3. Set the connection string:
   ```bash
   cd api
   dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=TaskTracker;Username=postgres;Password=postgres"
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
