using FluentOperation;
using FluentOperationUsage;


var op = new OperationSample();

var res = OperationBuilder
    .CreateOperation<string>()
    .SetAsyncOperation(async () => await op.SayHello("ss"))
    .SetChallenge(rs=>rs.Contains("named"),"this is unnamed!")
    .SetAndSuccessIf(rs=>rs.Contains("Well"),"has not well")
    .ExecuteAsync();

Console.WriteLine((await res).Result);