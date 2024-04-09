namespace FluentOperation;

public class OperationResult<TResult>
{
    public bool IsSuccess => Failures.Count == 0;
    public TResult? Result { get; init; }
    public List<OperationFailure> Failures { get; init; } = new();

    public override string ToString()
        => $"Status : {IsSuccess} - Result : {Result} - Failure : {Failures.Count}";

    public String UserFailure()
        => string.Join(" - ", Failures.Select(s => s.UserMessage));
    
    public static OperationResult<TResult> Success() => new();

    public static OperationResult<TResult> Failed(IEnumerable<OperationFailure> errors) => new()
    {
        Failures = errors.ToList()
    };
}