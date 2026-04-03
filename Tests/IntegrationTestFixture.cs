using DataAccessor;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Shared.Interfaces.DataManagement.DataAccessor;

namespace Tests;

public class IntegrationTestFixture : IAsyncLifetime
{
    private const string TestDbName = "TaskTracker_Test";
    private const string BaseConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres";
    private const string TestConnectionString = $"{BaseConnectionString};Database={TestDbName}";

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await CreateTestDatabase();
        await SeedDatabase();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration(config =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:Default"] = TestConnectionString,
                        ["Jwt:Key"] = "integration-test-key-that-is-at-least-32-bytes-long",
                        ["Jwt:Issuer"] = "TestIssuer",
                        ["Jwt:Audience"] = "TestAudience"
                    });
                });
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IDataAccessorOptions));
                    if (descriptor is not null)
                        services.Remove(descriptor);

                    services.AddSingleton<IDataAccessorOptions>(
                        _ => new DataAccessorOptions(TestConnectionString));
                });
            });

        Client = Factory.CreateClient();
    }

    private static async Task CreateTestDatabase()
    {
        await using NpgsqlConnection connection = new(BaseConnectionString + ";Database=postgres");
        await connection.OpenAsync();

        // Drop if exists from a previous failed run
        await using (NpgsqlCommand dropCmd = new(
            $"DROP DATABASE IF EXISTS \"{TestDbName}\" WITH (FORCE);", connection))
        {
            await dropCmd.ExecuteNonQueryAsync();
        }

        await using (NpgsqlCommand createCmd = new(
            $"CREATE DATABASE \"{TestDbName}\";", connection))
        {
            await createCmd.ExecuteNonQueryAsync();
        }
    }

    private static async Task SeedDatabase()
    {
        await using NpgsqlConnection connection = new(TestConnectionString);
        await connection.OpenAsync();

        string[] sqlFiles =
        [
            "SQL/Migrations/001_CreateUsersTable.pgsql",
            "SQL/Migrations/002_CreateRefreshTokenTable.pgsql",
            "SQL/StoredProcedures/users/get_all_users.pgsql",
            "SQL/StoredProcedures/users/get_users_by_uid.pgsql",
            "SQL/StoredProcedures/users/get_users_by_email.pgsql",
            "SQL/StoredProcedures/users/create_user.pgsql",
            "SQL/StoredProcedures/users/delete_users.pgsql",
            "SQL/StoredProcedures/users/update_users.pgsql",
            "SQL/StoredProcedures/refresh_tokens/create_refresh_token.pgsql",
            "SQL/StoredProcedures/refresh_tokens/get_refresh_token.pgsql",
            "SQL/StoredProcedures/refresh_tokens/delete_refresh_token.pgsql",
            "SQL/StoredProcedures/refresh_tokens/delete_refresh_tokens_by_user.pgsql"
        ];

        foreach (string file in sqlFiles)
        {
            string sql = await File.ReadAllTextAsync(
                Path.Combine(GetProjectRoot(), file));
            await using NpgsqlCommand cmd = new(sql, connection);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    private static string GetProjectRoot()
    {
        string dir = Directory.GetCurrentDirectory();
        while (dir is not null && !File.Exists(Path.Combine(dir, "api.sln")))
            dir = Directory.GetParent(dir)!.FullName;
        return dir ?? throw new InvalidOperationException("Could not find project root");
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();

        // Drop the test database
        await using NpgsqlConnection connection = new(BaseConnectionString + ";Database=postgres");
        await connection.OpenAsync();
        await using NpgsqlCommand cmd = new(
            $"DROP DATABASE IF EXISTS \"{TestDbName}\" WITH (FORCE);", connection);
        await cmd.ExecuteNonQueryAsync();
    }
}
