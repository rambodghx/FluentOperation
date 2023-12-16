namespace FluentOperation;

public static class DemandOperationExtension
{
    public static Task<DemandOperation<TResult>> ExecuteAsync<TResult>(
        this DemandOperation<TResult> operation,
        Task<TResult> task
    ) where TResult : class
    {
        return operation.ExecuteAsync(() => task);
    }

    public static async Task<DemandOperation<TResult>> ExecuteAsync<TResult>(
        this Task<DemandOperation<TResult>> operationTask,
        Task<TResult> task
    ) where TResult : class
    {
        var opTask = await operationTask;
        await opTask.ExecuteAsync(task);
        return opTask;
    }

    public static async Task<DemandOperation<TResult>> FlatException<TResult>(
        this Task<DemandOperation<TResult>> operationTask,
        Func<Exception, string> exceptionLambda
    ) where TResult : class
    {
        var opTask = await operationTask;
        opTask.FlatException(exceptionLambda);
        return opTask;
    }
    
    public static async Task<OperationResult<TResult>> GetResult<TResult>(
        this Task<DemandOperation<TResult>> operationTask
    ) where TResult : class
    {
        var opTask = await operationTask;
        return opTask.GetResult();
    }
}