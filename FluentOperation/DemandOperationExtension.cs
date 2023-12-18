namespace FluentOperation;

public static class DemandOperationExtension
{
    public static Task<DemandOperation<TResult>> ExecuteAsync<TResult>(
        this DemandOperation<TResult> operation,
        Task<TResult> task
    )
    {
        return operation.ExecuteAsync(() => task);
    }

    public static async Task<DemandOperation<TResult>> ExecuteAsync<TResult>(
        this Task<DemandOperation<TResult>> operationTask,
        Task<TResult> task
    )
    {
        var opTask = await operationTask;
        await opTask.ExecuteAsync(task);
        return opTask;
    }

    public static async Task<DemandOperation<TResult>> FlatException<TResult>(
        this Task<DemandOperation<TResult>> operationTask,
        Func<Exception, string> exceptionLambda
    )
    {
        var opTask = await operationTask;
        opTask.FlatException(exceptionLambda);
        return opTask;
    }
    
    public static async Task<OperationResult<TResult>> GetResult<TResult>(
        this Task<DemandOperation<TResult>> operationTask
    )
    {
        var opTask = await operationTask;
        return opTask.GetResult();
    }
}