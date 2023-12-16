using System.Threading.Tasks.Dataflow;
using System.Transactions;
using FluentOperation;
using FluentOperationUsage;

var res = await OperationBuilder.CreateDemandOperation<string>()
    .ExecuteAsync(()=>Task.FromResult("throw new InvalidCastException(\"Wow !!!\")"))
    .FlatException(ex=>ex switch
    {
        InvalidDataException => "Invalid data",
        _ => "Unknown error"
    })
    .GetResult();

Console.WriteLine(res);