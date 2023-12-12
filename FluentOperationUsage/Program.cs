using FluentOperation;
using FluentOperationUsage;


var op = new OperationSample();

var res = OperationBuilder
    .CreateOperation<string>()
    .SetOperation(() => op.SayHello("well"))
    .SetOrSuccessIf(re => !re.Any(),"There is text!!")
    .Execute();

Console.WriteLine(res);