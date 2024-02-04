using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading.Tasks.Dataflow;

namespace FluentOperation;

public class DemandOperation<TResult>
{
    private List<Exception> _exceptions = new();
    public ReadOnlyCollection<Exception> Exceptions => _exceptions.AsReadOnly();
    private Func<Exception, string>? _exceptionFlatterLambda;
    private Action<Exception>? _exceptionEventLambda;
    private bool _isBreak = false;
    private TResult? _operationResult;

    public async Task<DemandOperation<TResult>> BreakIfAsync(Func<Task<bool>> breakLambda)
    {
        if (_operationResult is not null)
            throw new InvalidOperationException("Break logic must be defined before Execute");
        try
        {
            _isBreak = await breakLambda();
            if (_isBreak) throw new VerificationException("Break logic executed");
        }
        catch (Exception e)
        {
            _isBreak = true;
            _exceptions.Add(e);
        }

        return this;
    }

    public DemandOperation<TResult> BreakIf(Func<bool> breakLambda)
    {
        if (_operationResult is not null)
            throw new InvalidOperationException("Break logic must be defined before Execute");
        try
        {
            _isBreak = breakLambda();
            if (_isBreak) throw new VerificationException("Break logic executed");
        }
        catch (Exception e)
        {
            _isBreak = true;
            _exceptions.Add(e);
        }

        return this;
    }

    public DemandOperation<TResult> Execute(Func<TResult> mainLambda)
    {
        if (_isBreak) return this;
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
        if (_isBreak) return this;
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

    public DemandOperation<TResult> OnException(Action<Exception> exceptionLambda)
    {
        if (_exceptionEventLambda is not null)
            throw new InvalidOperationException("Exception event has set before");
        _exceptionEventLambda = exceptionLambda;
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

        RaiseExceptionEvent();
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

        // Raise action on each of them
        void RaiseExceptionEvent()
            => _exceptions.ForEach(ex =>
            {
                _exceptionEventLambda ??= _ => { };
                _exceptionEventLambda(ex);
            });
    }
}