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
}