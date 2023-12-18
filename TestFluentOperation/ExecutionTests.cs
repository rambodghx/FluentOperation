using FluentOperation;

namespace TestFluentOperation;

public class ExecutionTests
{
    [Theory]
    [InlineData("First Value")]
    [InlineData("Second Value")]
    [InlineData("Third Value")]
    public void Is_SyncExecution_Works(string value)
    {
        var operation = OperationBuilder.CreateDemandOperation<string>()
            .Execute(() => value)
            .GetResult();
        Assert.True(operation.IsSuccess);
        Assert.NotNull(operation.Result);
        Assert.Equal(value,operation.Result);
    }
    
    [Theory]
    [InlineData("First Value")]
    [InlineData("Second Value")]
    [InlineData("Third Value")]
    public async Task Is_AsyncExecution_Works(string value)
    {
        var operation = await OperationBuilder.CreateDemandOperation<string>()
            .ExecuteAsync(Task.FromResult(value))
            .GetResult();
        Assert.True(operation.IsSuccess);
        Assert.NotNull(operation.Result);
        Assert.Equal(value,operation.Result);
    }
}