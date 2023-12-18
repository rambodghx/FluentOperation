using System.Threading.Tasks.Dataflow;
using System.Transactions;
using FluentOperation;
using FluentOperationUsage;

var ac = new BufferBlock<string>();
var dc = new TransformBlock<string, int>(ac => ac.Length);
ac.LinkTo(dc);
ac.Post("hm");
ac.Post("hm2");
dc.LinkTo(new ActionBlock<int>(Console.WriteLine));
ac.Completion.Wait();