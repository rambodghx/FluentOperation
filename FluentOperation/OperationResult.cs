namespace FluentOperation;

public class OperationResult<TResult>
{
    public bool IsSuccess => Failures.Count == 0;
    public TResult? Result { get; init; }
    public List<OperationFailure> Failures { get; } = new();
    public override string ToString() 
        => $"Status : {IsSuccess} - Result : {Result} - Failure : {Failures.Count}";

    public String UserFailure() 
        => string.Join(" - ", Failures.Select(s => s.UserMessage));
}