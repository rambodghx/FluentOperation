namespace FluentOperation;

/*
 In this version I'm trying to develop the base on fluent
 */
public static class OperationBuilder
{
    public static Operation<TResult> CreateOperation<TResult>()
    {
        var instance = new Operation<TResult>();
        return instance;
    }

    public static DemandOperation<TResult> CreateDemandOperation<TResult>()
    {
        return new DemandOperation<TResult>();
    }
}