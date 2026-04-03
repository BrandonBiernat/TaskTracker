namespace Shared.Interfaces.ReturnResults;

public interface IOperationResult {
    bool HasSuccessStatus { get; }
    string Message { get; }
}

public interface IOperationResult<T> : IOperationResult {
    T? Payload { get; }
}