using FluentOperation;
using FluentOperationUsage;


var op = new OperationSample();

var res = OperationBuilder
    .CreateOperation<string>()
    .SetOperation(() => op.SayHello("well"))
    .SetChallenge(rs=>rs.Contains("we"),"There is no we")
    .SetAndSuccessIf(rs=>rs.EndsWith("l"),"has l in the end")
    .SetAndSuccessIf(rs=>rs.Contains("Hello"),"Has not hellow")
    .Execute();

Console.WriteLine(res.Failure?.UserMessage ?? res.Result);