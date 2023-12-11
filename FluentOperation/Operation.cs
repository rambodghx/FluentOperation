namespace FluentOperation;

public class Operation<TResult> where TResult : class
{
    private Func<TResult> _operationLambda;

    public void SetOperation(Func<TResult> operationLambda)
    {
        _operationLambda = operationLambda;
    }

    public OperationResult<TResult> Execute()
    {
        if (_operationLambda is null)
            throw new InvalidOperationException("Operation lambda must be set before execute operation");
        try
        {
            var operationResult = _operationLambda();
            return new OperationResult<TResult>
            {
                Result = operationResult
            };
        }
        catch (Exception e)
        {
            return new OperationResult<TResult>
            {
                Failure = new OperationFailure
                {
                    Exception = e,
                    UserMessage = "Not Proved"
                }
            };
        }
    }
}