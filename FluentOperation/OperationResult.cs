namespace FluentOperation;

public class OperationResult<TResult> where TResult : class
{
    public bool IsSuccess => Failure is null;
    public TResult? Result { get; init; }
    public OperationFailure? Failure { get; set; }

    public override string ToString() 
        => $"Status : {IsSuccess} - Result : {Result} - Failure : {Failure?.UserMessage ?? "No Failure"}";
}