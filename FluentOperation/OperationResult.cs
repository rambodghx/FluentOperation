namespace FluentOperation;

public class OperationResult<TResult> where TResult : class
{
    public bool IsSuccess => Failure is null;
    public TResult? Result { get; init; }
    public OperationFailure? Failure { get; init; }
}