using System.Linq.Expressions;

namespace FluentOperation;

public sealed class Operation<TResult> where TResult : class
{
    private Func<TResult>? _operationLambda;
    private Func<Exception, string>? _exceptionLambda;
    private (Func<TResult, bool> challenge, string failedChallengeMessage)? _mainChallenge = null;

    private readonly Queue<Func<TResult, bool>> _orSuccessChallenges = new();
    private readonly Queue<(Func<TResult, bool> challenge, string failedMessage)> _andSuccessChallenges = new();
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
    /// Main challenge will be executed against the result of main operation leave it blank if you want to handle challenges with exceptions
    /// </summary>
    /// <param name="challengeLambda">Lambda that indicates the challenge against result</param>
    /// <param name="failedChallengeMessage">Failed user-friendly message when the challenge aborted</param>
    /// <returns></returns>
    public Operation<TResult> SetChallenge(Func<TResult, bool> challengeLambda, string failedChallengeMessage)
    {
        _mainChallenge = new()
        {
            challenge = challengeLambda,
            failedChallengeMessage = failedChallengeMessage
        };
        return this;
    }

    /// <summary>
    /// Add or logic challenge against result with main challenge that created on execution phase.
    /// Consider that the challenge lambda must not throw any exception.
    /// The main challenge must be set
    /// </summary>
    /// <param name="challengeLambda">Or Challenge Lambda</param>
    /// <exception cref="InvalidOperationException">When the main challenge has not set</exception>
    public Operation<TResult> SetOrSuccessIf(Func<TResult, bool> challengeLambda)
    {
        if (_mainChallenge is null)
            throw new InvalidOperationException("There is no main challenge to make it Or logic");
        _orSuccessChallenges.Enqueue((challengeLambda));
        return this;
    }

    /// <summary>
    /// Add and logic challenge against result with main challenge that created on execution phase.
    /// Consider that the challenge lambda must not throw any exception.
    /// The main challenge must be set
    /// </summary>
    /// <param name="challengeLambda">Or Challenge Lambda</param>
    /// <exception cref="InvalidOperationException">When the main challenge has not set</exception>
    /// <para name="failedMessage">User-Friendly Failed Message</para>
    public Operation<TResult> SetAndSuccessIf(Func<TResult, bool> challengeLambda, string failedMessage)
    {
        if (_mainChallenge is null)
            throw new InvalidOperationException("There is no main challenge to make it Or logic");
        _andSuccessChallenges.Enqueue((challengeLambda, failedMessage));
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
        var mainChallengeStatus = ExecuteMainChallenge(operationResult);
        var andChallengesResult = ExecuteSuccessAndChallenge(operationResult, mainChallengeStatus);
        var finalResult = ExecuteSuccessOrChallenges(operationResult, andChallengesResult);
        return finalResult;
    }

    private ChallengeResult ExecuteMainChallenge(OperationResult<TResult> operationResult)
    {
        if (operationResult is { IsSuccess: false } or { Result: null } || _mainChallenge is null)
            return new ChallengeResult
            {
                IsSuccess = true
            };

        var challengeResult = _mainChallenge.Value.challenge(operationResult.Result);
        return new ChallengeResult
        {
            IsSuccess = challengeResult,
            ChallengeResultMessage = _mainChallenge.Value.failedChallengeMessage
        };
    }

    private OperationResult<TResult> ExecuteSuccessOrChallenges(
        OperationResult<TResult> operationResult,
        ChallengeResult mainChallengeResult)
    {
        if (mainChallengeResult.IsSuccess) return operationResult;

        var overallStatus = false;
        foreach (var challenge in _orSuccessChallenges)
        {
            var challengeResult = challenge(operationResult.Result!); //Result is not able being null 
            overallStatus = challengeResult;
            if (challengeResult) break; // Short circuit
        }

        return new OperationResult<TResult>
        {
            Result = overallStatus ? operationResult.Result : null,
            Failure = overallStatus ? null : new OperationFailure(mainChallengeResult.ChallengeResultMessage)
        };
    }

    private ChallengeResult ExecuteSuccessAndChallenge(
        OperationResult<TResult> operationResult,
        ChallengeResult mainChallengeResult)
    {
        if (!mainChallengeResult.IsSuccess)
            return new ChallengeResult
            {
                IsSuccess = false,
                ChallengeResultMessage = mainChallengeResult.ChallengeResultMessage
            };

        var (overallStatus, failedMessage) = (true, "");
        foreach (var challengeWrapper in _andSuccessChallenges)
        {
            var challengeResult = challengeWrapper.challenge(operationResult.Result!);
            (overallStatus, failedMessage) = (challengeResult, challengeWrapper.failedMessage);
            if (!overallStatus) break; //Looking for false to short-circuit
        }

        return new ChallengeResult
        {
            IsSuccess = overallStatus,
            ChallengeResultMessage = failedMessage
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