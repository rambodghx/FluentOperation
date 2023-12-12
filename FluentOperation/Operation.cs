using System.Linq.Expressions;

namespace FluentOperation;

public class Operation<TResult> where TResult : class
{
    private Func<TResult>? _operationLambda;
    private Func<Exception, string>? _exceptionLambda;
    private readonly Queue<(Func<TResult, bool> challenge, string failedMessage)> _orSuccessChallenges = new();
    public bool IsExecuted { private set; get; }

    /// <summary>
    /// Set your main operation that must be run on execution
    /// </summary>
    /// <param name="operationLambda">Operation Lambda</param>
    /// <exception cref="InvalidOperationException">The operation lambda is not mutable</exception>
    public Operation<TResult> SetOperation(Func<TResult> operationLambda)
    {
        if (_operationLambda is not null)
            throw new InvalidOperationException("Operation lambda has been set before");
        _operationLambda = operationLambda;
        return this;
    }

    /// <summary>
    /// Add or logic challenge against result that created on execution phase.
    /// Consider that the challenge lambda must not throw any exception.
    /// </summary>
    /// <param name="challengeLambda">Or Challenge Lambda</param>
    public Operation<TResult> SetOrSuccessIf(Func<TResult, bool> challengeLambda, string challengeFailedMessage)
    {
        _orSuccessChallenges.Enqueue((challengeLambda, challengeFailedMessage));
        return this;
    }

    /// <summary>
    /// Provide proper lambda to handle exception user-friendly message.
    /// Consider that this lambda must not throw any exception
    /// </summary>
    /// <param name="exceptionLambda">Exception Message Flatter</param>
    public Operation<TResult> SetExceptionFlatter(Func<Exception, string> exceptionLambda)
    {
        _exceptionLambda = exceptionLambda;
        return this;
    }

    /// <summary>
    /// Executing all the operation logics that provided for once.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">There is no provided main operation</exception>
    /// <exception cref="InvalidOperationException">The execution has been ran before</exception>
    public OperationResult<TResult> Execute()
    {
        if (IsExecuted)
            throw new InvalidOperationException("The operation has been ran before");
        if (_operationLambda is null)
            throw new InvalidOperationException("Operation lambda must be set before execute operation");

        var operationResult = ExecuteOperation();
        var finalResult = ExecuteSuccessOrChallenges(operationResult);

        return finalResult;
    }

    private OperationResult<TResult> ExecuteSuccessOrChallenges(OperationResult<TResult> operationResult)
    {
        if (operationResult is { IsSuccess: false } or { Result: null } || !_orSuccessChallenges.Any())
            return operationResult;
        ChallengeResult finalChallengeResult = null;
        foreach (var challengeWrapper in _orSuccessChallenges)
        {
            var challengeResult = challengeWrapper.challenge(operationResult.Result);
            finalChallengeResult = new ChallengeResult
            {
                IsSuccess = challengeResult,
                ChallengeResultMessage = challengeWrapper.failedMessage
            };
            if (!challengeResult) break; // Short circuit
        }

        return new OperationResult<TResult>
        {
            Result = finalChallengeResult!.IsSuccess ? operationResult.Result : null,
            Failure = finalChallengeResult!.IsSuccess
                ? null
                : new OperationFailure(userMessage: finalChallengeResult!.ChallengeResultMessage)
        };
    }

    private OperationResult<TResult> ExecuteOperation()
    {
        try
        {
            var result = _operationLambda!();
            return new OperationResult<TResult>
            {
                Result = result
            };
        }
        catch (Exception e)
        {
            String? handledUserMessage = null;
            if (_exceptionLambda is not null)
                handledUserMessage = _exceptionLambda(e);
            return new OperationResult<TResult>
            {
                Failure = new OperationFailure
                {
                    Exception = e,
                    UserMessage = handledUserMessage
                }
            };
        }
        finally
        {
            IsExecuted = true;
        }
    }
}