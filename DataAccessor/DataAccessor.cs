using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared;
using Shared.Interfaces.DataManagement.DataAccessor;
using Shared.Interfaces.ReturnResults;
using Shared.ReturnResult;

namespace DataAccessor;

public class DataAccessorOptions(string connectionString) : IDataAccessorOptions {
    public string ConnectionString { get; } = connectionString;
}

public class DataAccessor(
    IDataAccessorOptions options,
    ILogger<IDataAccessor> logger) : IDataAccessor {

    public async Task<IOperationResult> ExecuteAsync(
        string function,
        object? parameters)
    {
        try {
            using NpgsqlConnection connection = new (options.ConnectionString);
            string sql = BuildFunctionCall(function, parameters);
            await connection.ExecuteAsync(
                sql: sql,
                param: parameters,
                commandType: CommandType.Text);
            return OperationResult.Success();
        } catch (Exception ex) {
            logger.LogError(ex, "Failed to execute {Function}", function);
            return OperationResult.Failure("An error occurred when executing the operation");
        }
    }

    public async Task<IOperationResult<IEnumerable<T>>> QueryAsync<T>(
        string function,
        object? parameters = null)
    {
        try {
            using NpgsqlConnection connection = new (options.ConnectionString);
            string sql = BuildFunctionCall(function, parameters);
            IEnumerable<T> results = await
                connection
                .QueryAsync<T>(
                    sql: sql,
                    param: parameters,
                    commandType: CommandType.Text);
            return OperationResult<IEnumerable<T>>.Success(results);
        } catch (Exception ex) {
            logger.LogError(ex, "Failed to execute {Function}", function);
            return OperationResult<IEnumerable<T>>.Failure("An error occurred querying the database");
        }
    }

    public async Task<IOperationResult<IEnumerable<T>>> QueryAsync<T>(
        SQLQuery sql, 
        object? parameters = null)
    {
        try {
            using NpgsqlConnection connection = new (options.ConnectionString);
            IEnumerable<T> results = await
                connection
                .QueryAsync<T>(
                    sql: sql.Value,
                    param: parameters,
                    commandType: CommandType.Text);
            return OperationResult<IEnumerable<T>>.Success(results);
        } catch (Exception ex) {
            logger.LogError(ex, "Failed to execute {SqlQuery}", sql.Value);
            return OperationResult<IEnumerable<T>>.Failure("An error occurred querying the database");
        }
    }

    private static string BuildFunctionCall(string function, object? parameters)
    {
        if (parameters is null)
            return $"SELECT * FROM {function}()";

        var paramNames = parameters.GetType()
            .GetProperties()
            .Select(p => $"@{p.Name}");

        return $"SELECT * FROM {function}({string.Join(", ", paramNames)})";
    }
}