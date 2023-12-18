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
        ChooseExceptionStrategy();
        var errors = GetOccuredExceptions();
        result.Failures.AddRange(errors);
        
        return result;
        // Provide general exception flatter if exception flatter is not defined 
        void ChooseExceptionStrategy()
        {
            if (_exceptionFlatter is not null) return;
            _exceptionFlatter = new TransformBlock<Exception, OperationFailure>(ac => new OperationFailure
            {
                Exception = ac,
                UserMessage = ac.Message
            });
        } 
        // Return all of occured exception
        List<OperationFailure> GetOccuredExceptions()
        {
            _exceptions.Complete();
            if (_exceptionFlatter!.TryReceiveAll(out var errors))
                return errors?.ToList() ?? new List<OperationFailure>();
            return new List<OperationFailure>();
        }
    }
}