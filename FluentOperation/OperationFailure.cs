namespace FluentOperation;

public class OperationFailure
{
    public OperationFailure()
    {
    }

    public OperationFailure(string? userMessage)
    {
        UserMessage = userMessage;
    }

    public string? UserMessage { get; init; }
    public Exception? Exception { get; init; }
}