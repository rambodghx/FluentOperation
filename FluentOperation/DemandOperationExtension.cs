namespace FluentOperation;

public static class DemandOperationExtension
{
    public static async Task<DemandOperation<TResult>> BreakIfAsync<TResult>(
        this Task<DemandOperation<TResult>> operationTask,
        Func<Task<bool>> breakLambda,
        string? breakMessage = null)
    {
        var opTask = await operationTask;
        await opTask.BreakIfAsync(breakLambda,breakMessage);
        return opTask;
    }

    public static async Task<DemandOperation<TResult>> Execute<TResult>(
        this Task<DemandOperation<TResult>> operationTask,
        Func<TResult> lambda)
    {
        var opTask = await operationTask;
        opTask.Execute(lambda);
        return opTask;
    }

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

    public static async Task<DemandOperation<TResult>> ExecuteAsync<TResult>(
        this Task<DemandOperation<TResult>> operationTask,
        Func<Task<TResult>> task
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

    public static async Task<DemandOperation<TResult>> OnException<TResult>(
        this Task<DemandOperation<TResult>> operationTask,
        Action<Exception> exceptionLambda
    )
    {
        var opTask = await operationTask;
        opTask.OnException(exceptionLambda);
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