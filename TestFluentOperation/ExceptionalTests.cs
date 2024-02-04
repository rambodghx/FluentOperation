using FluentOperation;
using Xunit.Sdk;

namespace TestFluentOperation;

public class ExceptionalTests
{
    [Fact]
    public void Is_DoubleAdd_ExceptionFlatter_ThrowException()
    {
        var operation = OperationBuilder.CreateDemandOperation<string>();
        operation.FlatException(_ => "");
        Assert.Throws<InvalidOperationException>(() =>
        {
            operation.FlatException(_ => "");
        });
    }

    [Fact]
    public void Is_Uninitialized_GetResult_ThrowException()
    {
        var operation = OperationBuilder.CreateDemandOperation<string>();
        Assert.Throws<InvalidOperationException>(() =>
        {
            operation.GetResult();
        });
    }

    [Fact]
    public void Is_ExceptionFlatter_Works_Unordered()
    {
        var operation = OperationBuilder.CreateDemandOperation<string>();
        operation.Execute(() => throw new EmptyException(""));
        operation.FlatException(ex => ex switch
        {
            EmptyException => "Empty",
            _ => "Unknown"
        });
        var result = operation.GetResult();
        Assert.False(result.IsSuccess);
        Assert.Null(result.Result);
        Assert.Single(result.Failures);
        Assert.NotEqual("Unknown",result.Failures.FirstOrDefault()!.UserMessage!);
    }
    [Fact]
    public void Is_ExceptionFlatter_Works_Ordered()
    {
        var operation = OperationBuilder.CreateDemandOperation<string>();
        operation.FlatException(ex => ex switch
        {
            EmptyException => "Empty",
            _ => "Unknown"
        });
        operation.Execute(() => throw new EmptyException(""));
        var result = operation.GetResult();
        Assert.False(result.IsSuccess);
        Assert.Null(result.Result);
        Assert.Single(result.Failures);
        Assert.NotEqual("Unknown",result.Failures.FirstOrDefault()!.UserMessage!);
    }

    [Fact]
    public void Is_InFlattedExceptions_Works()
    {
        var operation = OperationBuilder.CreateDemandOperation<string>();
        operation.FlatException(ex => ex switch
        {
            EmptyException => "Empty",
            _ => "Unknown"
        });
        operation.Execute(() => throw new NullException(""));
        var result = operation.GetResult();
        Assert.False(result.IsSuccess);
        Assert.Null(result.Result);
        Assert.Single(result.Failures);
        Assert.NotEqual("Empty",result.Failures.FirstOrDefault()!.UserMessage!);
    }

    [Fact]
    public void Is_UnhandledExceptions_Works()
    {
        var operation = OperationBuilder.CreateDemandOperation<string>();
        operation.Execute(() => throw new NullException("Nulled"));
        var result = operation.GetResult();
        Assert.False(result.IsSuccess);
        Assert.Null(result.Result);
        Assert.Single(result.Failures);
        Assert.NotNull(result.Failures.FirstOrDefault()!.UserMessage);
    }

    [Fact]
    public void Is_ExceptionAction_Works()
    {
        int errorsCount = 0;
        var operation = OperationBuilder.CreateDemandOperation<string>();
        operation.Execute(() => throw new NullException("Nulled"));
        operation.Execute(() => throw new NullException("Nulled"));
        operation.OnException((ex) => errorsCount++);
        var result = operation.GetResult();
        Assert.False(result.IsSuccess);
        Assert.Null(result.Result);
        Assert.Equal(2,result.Failures.Count);
        Assert.NotNull(result.Failures.FirstOrDefault()!.UserMessage);
        Assert.Equal(2,errorsCount);
    }

    [Fact]
    public void Is_BreakIf_Works()
    {
        var operation = OperationBuilder.CreateDemandOperation<string>()
            .BreakIf(() => true)
            .Execute(() => "Done")
            .GetResult();
        Assert.False(operation.IsSuccess);
        Assert.Null(operation.Result);
        Assert.NotNull(operation.Failures.FirstOrDefault()!.UserMessage);
    }
    [Fact]
    public void Is_BreakIfThrow_Works()
    {
        var operation = OperationBuilder.CreateDemandOperation<string>()
            .BreakIf(() => throw new InvalidOperationException())
            .Execute(() => "Done")
            .GetResult();
        Assert.False(operation.IsSuccess);
        Assert.Null(operation.Result);
        Assert.NotNull(operation.Failures.FirstOrDefault()!.UserMessage);
    }
    [Fact]
    public async Task Is_BreakIfAsync_Works()
    {
        var operation = await OperationBuilder.CreateDemandOperation<string>()
            .BreakIfAsync(() => Task.FromResult(true))
            .Execute(() => "Done")
            .GetResult();
        Assert.False(operation.IsSuccess);
        Assert.Null(operation.Result);
        Assert.NotNull(operation.Failures.FirstOrDefault()!.UserMessage);
    }
    [Fact]
    public async Task Is_BreakIfThrowAsync_Works()
    {
        var operation = await OperationBuilder.CreateDemandOperation<string>()
            .BreakIfAsync(() => throw new InvalidOperationException())
            .Execute(() => "Done")
            .GetResult();
        Assert.False(operation.IsSuccess);
        Assert.Null(operation.Result);
        Assert.NotNull(operation.Failures.FirstOrDefault()!.UserMessage);
    }
}