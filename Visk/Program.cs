using System.Diagnostics;
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
    ViskInstruction.Add(),
    ViskInstruction.CallForeign(printLong),

    ViskInstruction.PushConst(123),
    ViskInstruction.Ret()
});


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