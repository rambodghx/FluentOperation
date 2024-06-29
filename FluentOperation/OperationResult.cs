namespace FluentOperation;

public enum OperationStatus
{
    NotExecuted,
    DoneExecution,
    FailedExecution,
    Broken,
    GuardPassed
}
public class OperationResult<TResult>
{
    public bool IsSuccess => Failures.Count == 0;
    public TResult? Result { get; init; }
    public List<OperationFailure> Failures { get; init; } = new();

    public override string ToString()
        => $"Status : {IsSuccess} - Result : {Result} - Failure : {Failures.Count}";

    public String UserFailure()
        => string.Join(" - ", Failures.Select(s => s.UserMessage));

    public string[] FailuresAsArray() => Failures.Select(s => s.UserMessage).ToArray();

    public OperationResult()
    {
    }

    private OperationResult(TResult? result) => Result = result;
    public static OperationResult<TResult> Success(TResult result) => new(result);

    public static OperationResult<TResult> Failed(IEnumerable<OperationFailure> errors) => new()
    {
        Failures = errors.ToList()
    };
}