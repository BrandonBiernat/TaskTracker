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
        string storedProcedure, 
        object? parameters)
    {
        try {
            using NpgsqlConnection connection = new (options.ConnectionString);
            await connection.ExecuteAsync(
                sql: storedProcedure,
                param: parameters,
                commandType: CommandType.StoredProcedure);
            return OperationResult.Success();
        } catch (Exception ex) {
            logger.LogError(ex, "Failed to execute {StoredProcedure}", storedProcedure);
            return OperationResult.Failure("An error occurred when executing the operation");
        }
    }

    public async Task<IOperationResult<IEnumerable<T>>> QueryAsync<T>(
        string storedProcedure, 
        object? parameters = null)
    {
        try {
            using NpgsqlConnection connection = new (options.ConnectionString);
            IEnumerable<T> results = await
                connection
                .QueryAsync<T>(
                    sql: storedProcedure,
                    param: parameters,
                    commandType: CommandType.StoredProcedure);
            return OperationResult<IEnumerable<T>>.Success(results);
        } catch (Exception ex) {
            logger.LogError(ex, "Failed to execute {StoredProcedure}", storedProcedure);
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
}