using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

namespace FluentOperation;

public class DemandOperation<TResult>
{
    private List<Exception> _exceptions = new();
    public ReadOnlyCollection<Exception> Exceptions => _exceptions.AsReadOnly();
    private Func<Exception, string>? _exceptionFlatterLambda;
    private TResult? _operationResult;
    
    public DemandOperation<TResult> Execute(Func<TResult> mainLambda)
    {
        try
        {
            _operationResult = mainLambda();
        }
        catch (Exception e)
        {
            _exceptions.Add(e);
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
            _exceptions.Add(e);
        }

        return this;
    }

    public DemandOperation<TResult> FlatException(Func<Exception, string> exceptionLambda)
    {
        if (_exceptionFlatterLambda is not null)
            throw new InvalidOperationException("Exception lambda has set before");
        _exceptionFlatterLambda = exceptionLambda;
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
        if (result.Result is null && !errors.Any())
            throw new InvalidOperationException("Main operation is not defined");

        return result;

        // Provide general exception flatter if exception flatter is not defined 
        void ChooseExceptionStrategy() => _exceptionFlatterLambda ??= ex => ex.Message;

        // Return all of occured exception
        IEnumerable<OperationFailure> GetOccuredExceptions()
            => _exceptions.Select(ex => new OperationFailure
            {
                Exception = ex,
                UserMessage = _exceptionFlatterLambda!(ex)
            });
    }
}