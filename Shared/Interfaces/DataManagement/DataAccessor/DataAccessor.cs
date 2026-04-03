using Shared.Interfaces.ReturnResults;

namespace Shared.Interfaces.DataManagement.DataAccessor;

public interface IDataAccessorOptions {
    string ConnectionString { get; }
}

public interface IDataAccessor {
    Task<IOperationResult<IEnumerable<T>>> QueryAsync<T>(
        string storedProcedure,
        object? parameters = null);
    Task<IOperationResult<IEnumerable<T>>> QueryAsync<T>(
        SQLQuery sql,
        object? parameters = null);
    Task<IOperationResult> ExecuteAsync(
        string storedProcedure,
        object? parameters);
}