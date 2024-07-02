using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading.Tasks.Dataflow;

namespace FluentOperation;

public class DemandOperation<TResult>
{
    public OperationStatus OperationStatus { get; private set; }

    private List<Exception> _exceptions = new();
    public ReadOnlyCollection<Exception> Exceptions => _exceptions.AsReadOnly();
    private Func<Exception, string>? _exceptionFlatterLambda;
    private Action<Exception>? _exceptionEventLambda;
    private TResult? _operationResult;
    
    public DemandOperation<TResult> BreakIfThrowsAny(Action breakLambda, string breakMessage)
    {
        if (OperationStatus is OperationStatus.Broken) return this; // Providing and logic over chained breakIf
        if (OperationStatus is OperationStatus.Executed)
            throw new InvalidOperationException("Break logic must be defined before execute");
        try
        {
            breakLambda();
        }
        catch (Exception e)
        {
            OperationStatus = OperationStatus.Broken;
            _exceptions.Add(new VerificationException(breakMessage, e));
        }

        return this;
    }

    public async Task<DemandOperation<TResult>> BreakIfAsync(Func<Task<bool>> breakLambda, string? breakMessage = null)
    {
        if (OperationStatus is OperationStatus.Broken) return this; // Providing and logic over chained breakIf
        if (OperationStatus is OperationStatus.Executed)
            throw new InvalidOperationException("Break logic must be defined before Execute");
        try
        {
            if (await breakLambda())
            {
                OperationStatus = OperationStatus.Broken;
                throw new VerificationException(breakMessage ?? "Break logic executed");
            }
           
        }
        catch (Exception e)
        {
            OperationStatus = OperationStatus.Broken;
            _exceptions.Add(e);
        }

        return this;
    }

    public DemandOperation<TResult> BreakIf(Func<bool> breakLambda, string? breakMessage = null)
    {
        if (OperationStatus is OperationStatus.Broken) return this; // Providing and logic over chained breakIf
        if (OperationStatus is OperationStatus.Executed)
            throw new InvalidOperationException("Break logic must be defined before Execute");
        try
        {
            if (breakLambda())
            {
                OperationStatus = OperationStatus.Broken;
                throw new VerificationException(breakMessage ?? "Break logic executed");
            }
            
        }
        catch (Exception e)
        {
            OperationStatus = OperationStatus.Broken;
            _exceptions.Add(e);
        }

        return this;
    }

    public DemandOperation<TResult> Execute(Func<TResult> mainLambda)
    {
        if (OperationStatus is OperationStatus.Broken) return this;
        try
        {
            _operationResult = mainLambda();
        }
        catch (Exception e)
        {
            _exceptions.Add(e);
        }
        finally
        {
            OperationStatus = OperationStatus.Executed;
        }

        return this;
    }

    public async Task<DemandOperation<TResult>> ExecuteAsync(Func<Task<TResult>> mainLambda)
    {
        if (OperationStatus is OperationStatus.Broken) return this;
        try
        {
            _operationResult = await mainLambda();
        }
        catch (Exception e)
        {
            _exceptions.Add(e);
        }
        finally
        {
            OperationStatus = OperationStatus.Executed;
        }

        return this;
    }

    public DemandOperation<TResult> ReflectLowerExecution(Func<OperationResult<TResult>> lowerLambda)
    {
        if (OperationStatus is OperationStatus.Broken) return this;
        if (OperationStatus is OperationStatus.Executed)
            throw new InvalidOperationException("This method must be called instead of Execute()");
        try
        {
            var lowerRes = lowerLambda();
            _operationResult = lowerRes.Result;
            _exceptions.AddRange(lowerRes.Failures.Select(s => s.Exception));
        }
        catch (Exception e)
        {
            _exceptions.Add(e);
        }
        finally
        {
            OperationStatus = OperationStatus.Executed;
        }

        return this;
    }

    public async Task<DemandOperation<TResult>> ReflectLowerExecutionAsync(Func<Task<OperationResult<TResult>>> lowerLambda)
    {
        if (OperationStatus is OperationStatus.Broken) return this;
        if (OperationStatus is OperationStatus.Executed)
            throw new InvalidOperationException("This method must be called instead of Execute()");
        try
        {
            var lowerRes = await lowerLambda();
            _operationResult = lowerRes.Result;
            _exceptions.AddRange(lowerRes.Failures.Select(s => s.Exception));
        }
        catch (Exception e)
        {
            _exceptions.Add(e);
        }
        finally
        {
            OperationStatus = OperationStatus.Executed;
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
        if (result.Result is null && !errors.Any()) //todo check logic on value type returns
            throw new InvalidOperationException("Main operation is not defined");

        return result;

        // Provide general exception flatter if exception flatter is not defined 
        void ChooseExceptionStrategy() => _exceptionFlatterLambda ??= ex => ex.Message;

        // Return all of occured exception
        IEnumerable<OperationFailure> GetOccuredExceptions()
            => _exceptions.Select(ex => new OperationFailure(
                exception: ex,
                userMessage: _exceptionFlatterLambda!(ex)
            ));

        // Raise action on each of them
        void RaiseExceptionEvent()
            => _exceptions.ForEach(ex =>
            {
                _exceptionEventLambda ??= _ => { };
                _exceptionEventLambda(ex);
            });
    }
}