using System.Diagnostics;
using TestProject1;
using Visk;
using ViskCompiler;

// ReSharper disable UnusedVariable
var printLongs = typeof(Helper).GetMethod(nameof(Helper.PrintLongs));
var printLong = typeof(Helper).GetMethod(nameof(Helper.PrintLong));
var printSmt = typeof(Helper).GetMethod(nameof(Helper.PrintSmt));

var module = new ViskModule("main");

var fMain = module.AddFunction("main", 0, typeof(long));

fMain.RawInstructions.AddRange(new List<ViskInstruction>
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
    ViskInstruction.CallForeign(Tests.BigMethodInfo),

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