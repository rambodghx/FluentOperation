namespace FluentOperation;

/*
 In this version I'm trying to develop the base on fluent
 */
public static class OperationBuilder
{
    public static Operation<TResult> CreateOperation<TResult>() where TResult : class
    {
        var instance = new Operation<TResult>();
        return instance;
    }
}