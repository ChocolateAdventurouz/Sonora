namespace Sonora;

/// <summary>
/// Represent the result state of an operation.
/// </summary>
public sealed class OperationResult
{
    /// <summary>
    /// Result of the operation.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Message result of the operation.
    /// </summary>
    public string Message { get; }

    private OperationResult(bool isSuccess, string message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }

    public static OperationResult Success() => new OperationResult(true, "Operation completed successfully.");

    public static OperationResult Success(string message) => new OperationResult(true, message);

    public static OperationResult Failure(string message) => new OperationResult(false, message);

    public override string ToString() => Message;
}
