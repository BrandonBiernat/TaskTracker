using Shared.Interfaces.ReturnResults;

namespace Shared.ReturnResult;

public sealed class OperationResult<T> : IOperationResult<T>
{
    public bool HasSuccessStatus { get; }
    public string Message { get; }
    public T? Payload { get; }

    private OperationResult(
        bool success,
        string message,
        T? payload) {
        HasSuccessStatus = success;
        Message = message;
        Payload = payload;
    }

    public static OperationResult<T> Success(T payload) =>
        new (true, string.Empty, payload);
    public static OperationResult<T> Failure(string message) => 
        new (false, message, default);
}

public sealed class OperationResult : IOperationResult {
    public bool HasSuccessStatus { get; }
    public string Message { get; }

    private OperationResult(
        bool success,
        string message) {
        HasSuccessStatus = success;
        Message = message;
    }

    public static OperationResult Success() =>
        new(true, string.Empty);
    public static OperationResult Failure(string message) =>
        new(false, message);
}