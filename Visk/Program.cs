using System.Diagnostics;
using Visk;
using ViskCompiler;

var printLongs = typeof(Helper).GetMethod(nameof(Helper.PrintLongs));
var printLong = typeof(Helper).GetMethod(nameof(Helper.PrintLong));
var printSmt = typeof(Helper).GetMethod(nameof(Helper.PrintSmt));

var module = new ViskModule("main");

var fMain = module.AddFunction("main", 0, typeof(long));
var fOther = module.AddFunction("other", 10, typeof(long));

fMain.RawInstructions.AddRange(new List<ViskInstruction>
{
    ViskInstruction.Nop(),
    ViskInstruction.Nop(),
    ViskInstruction.Nop(),

    ViskInstruction.PushConst(1),
    ViskInstruction.PushConst(2),
    ViskInstruction.PushConst(3),
    ViskInstruction.PushConst(4),
    ViskInstruction.PushConst(5),
    ViskInstruction.PushConst(6),
    ViskInstruction.PushConst(7),
    ViskInstruction.PushConst(8),
    ViskInstruction.PushConst(9),
    ViskInstruction.PushConst(10),
    ViskInstruction.Call(fOther),

    ViskInstruction.Ret(),

    ViskInstruction.Nop(),
    ViskInstruction.Nop(),
    ViskInstruction.Nop()
});

fOther.RawInstructions.AddRange(new List<ViskInstruction>
{
    ViskInstruction.SetArg("arg0"),
    ViskInstruction.SetArg("arg1"),
    ViskInstruction.SetArg("arg2"),
    ViskInstruction.SetArg("arg3"),
    ViskInstruction.SetArg("arg4"),
    ViskInstruction.SetArg("arg5"),
    ViskInstruction.SetArg("arg6"),
    ViskInstruction.SetArg("arg7"),
    ViskInstruction.SetArg("arg8"),
    ViskInstruction.SetArg("arg9"),

    ViskInstruction.LoadLocal("arg0"),
    ViskInstruction.LoadLocal("arg9"),
    ViskInstruction.IMul(),
    ViskInstruction.IMul(),
    ViskInstruction.Ret()
});


var image = new ViskImage(module);
var executor = image.Compile();
var asmDelegate = executor.GetDelegate();

Console.WriteLine(new string('-', Console.WindowWidth));
Console.WriteLine(executor.ToString());
Console.WriteLine(new string('-', Console.WindowWidth));

var sw = new Stopwatch();
for (var i = 0; i < 5; i++)
{
    sw.Restart();

    Console.WriteLine($"Function returned: {asmDelegate()}; Approximate execution time: {sw.ElapsedMilliseconds}\n\n");
}