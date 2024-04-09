namespace FluentOperation;

public class OperationFailure
{
    public OperationFailure(string userMessage, Exception exception) =>
        (UserMessage, Exception) = (userMessage, exception);
    public OperationFailure(string userMessage) =>
        (UserMessage, Exception) = (userMessage, new Exception(userMessage));
    public string UserMessage { get; }
    public Exception Exception { get; }
}