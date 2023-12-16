namespace FluentOperationUsage;

public class OperationSample
{
    public OperationSample()
    {
        
    }

    public Task<string> SayHello(string status)
    {
        return Task.FromResult("Well done");
    }
    
}