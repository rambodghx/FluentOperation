using FluentOperation;
using FluentOperationUsage;


var op = new OperationSample();

var res = OperationBuilder
    .CreateOperation<string>()
    .SetOperation(() => op.SayHello("well"))
    .SetChallenge(rs=>!rs.Contains("ss"),"There is no ss")
    .SetOrSuccessIf(rs=>rs.Contains("we"))
    .Execute();

Console.WriteLine(res);