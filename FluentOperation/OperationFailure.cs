namespace FluentOperation;

public class OperationFailure
{
    public string UserMessage { get; init; } = null!;
    public Exception? Exception { get; init; }
}