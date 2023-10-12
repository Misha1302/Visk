using System.Diagnostics;
using Visk;
using ViskCompiler;

// ReSharper disable UnusedVariable
var printLongs = typeof(Helper).GetMethod(nameof(Helper.PrintLongs));
var printLong = typeof(Helper).GetMethod(nameof(Helper.PrintLong));
var printSmt = typeof(Helper).GetMethod(nameof(Helper.PrintSmt));

var module = new ViskModule("main");

var mf = module.AddFunction("main", 0, typeof(long));
var of = module.AddFunction("other", 10, typeof(long));

mf.RawInstructions.AddRange(
    new List<ViskInstruction>
    {
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
        ViskInstruction.Call(of),

        ViskInstruction.Ret()
    }
);

of.RawInstructions.AddRange(
    new List<ViskInstruction>
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
        ViskInstruction.LoadLocal("arg1"),
        ViskInstruction.IMul(),
        ViskInstruction.LoadLocal("arg2"),
        ViskInstruction.IMul(),
        ViskInstruction.LoadLocal("arg3"),
        ViskInstruction.IMul(),
        ViskInstruction.LoadLocal("arg4"),
        ViskInstruction.IMul(),
        ViskInstruction.LoadLocal("arg5"),
        ViskInstruction.IMul(),
        ViskInstruction.LoadLocal("arg6"),
        ViskInstruction.IMul(),
        ViskInstruction.LoadLocal("arg7"),
        ViskInstruction.IMul(),
        ViskInstruction.LoadLocal("arg8"),
        ViskInstruction.IMul(),
        ViskInstruction.LoadLocal("arg9"),
        ViskInstruction.IMul(),

        ViskInstruction.Ret()
    }
);

var image = new ViskImage(module);
var executor = image.Compile();
var asmDelegate = executor.GetDelegate();

var assemblerString = executor.ToString();
Console.WriteLine(new string('-', Console.WindowWidth));
Console.WriteLine(assemblerString);
Console.WriteLine(new string('-', Console.WindowWidth));
assemblerString.SaveFileAsync("debugInfo.txt");

var sw = new Stopwatch();
for (var i = 0; i < 1; i++)
{
    sw.Restart();

    Console.WriteLine($"Function returned: {asmDelegate()}; Approximate execution time: {sw.ElapsedMilliseconds}\n\n");
}