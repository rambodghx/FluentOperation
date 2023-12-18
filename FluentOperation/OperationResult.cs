namespace FluentOperation;

public class OperationResult<TResult> where TResult : class
{
    public bool IsSuccess => Failures.Count == 0;
    public TResult? Result { get; init; }
    // public OperationFailure? Failure { get; set; }
    public List<OperationFailure> Failures { get; } = new();
    public override string ToString() 
        => $"Status : {IsSuccess} - Result : {Result} - Failure : {Failures.Count}";
}