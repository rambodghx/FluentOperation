using System.Threading.Tasks.Dataflow;

namespace FluentOperation;

public class DemandOperation<TResult> where TResult : class
{
    private readonly BufferBlock<Exception> _exceptions = new();
    private TransformBlock<Exception, OperationFailure>? _exceptionFlatter;
    private TResult? _operationResult;

    public DemandOperation<TResult> Execute(Func<TResult> mainLambda)
    {
        try
        {
            _operationResult = mainLambda();
        }
        catch (Exception e)
        {
            _exceptions.Post(e);
        }

        return this;
    }

    public async Task<DemandOperation<TResult>> ExecuteAsync(Func<Task<TResult>> mainLambda)
    {
        try
        {
            _operationResult = await mainLambda();
        }
        catch (Exception e)
        {
            _exceptions.Post(e);
        }

        return this;
    }

    public DemandOperation<TResult> FlatException(Func<Exception, string> exceptionLambda)
    {
        if (_exceptionFlatter is not null)
            throw new InvalidOperationException("Exception lambda has set before");
        _exceptionFlatter = new(ex => new OperationFailure
        {
            Exception = ex,
            UserMessage = exceptionLambda(ex)
        });
        _exceptions.LinkTo(_exceptionFlatter);
        return this;
    }

    public OperationResult<TResult> GetResult()
    {
        var result = new OperationResult<TResult>
        {
            Result = _operationResult
        };
        if (_exceptionFlatter is not null)
        {
            _exceptionFlatter.Complete();
            _exceptionFlatter.TryReceiveAll(out var errors);
            result.Failure = errors is null || errors.Count == 0
                ? null
                : new OperationFailure(string.Join(',', errors.Select(s => s.UserMessage)));
        }

        return result;
    }
}