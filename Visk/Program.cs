using System.Diagnostics;
using Visk;
using ViskCompiler;

// ReSharper disable UnusedVariable
var printLongs = typeof(Helper).GetMethod(nameof(Helper.PrintLongs));
var printLong = typeof(Helper).GetMethod(nameof(Helper.PrintLong));
var printDouble = typeof(Helper).GetMethod(nameof(Helper.PrintDouble));
var printDoubles = typeof(Helper).GetMethod(nameof(Helper.PrintDoubles));
var printSmt = typeof(Helper).GetMethod(nameof(Helper.PrintSmt));

var module = new ViskModule("main");

var mf = module.AddFunction("main", new List<Type>(0), typeof(long));

mf.RawInstructions.AddRange(
    new List<ViskInstruction>
    {
        ViskInstruction.PushConstD(1),
        ViskInstruction.PushConstD(3),
        ViskInstruction.DivD(),
        
        ViskInstruction.PushConstD(1.43),
        ViskInstruction.PushConstD(2.32),
        ViskInstruction.MulD(),

        ViskInstruction.PushConstD(2),
        ViskInstruction.PushConstD(2),
        ViskInstruction.AddD(),
        
        ViskInstruction.PushConstD(3),
        ViskInstruction.PushConstD(4),
        ViskInstruction.SubD(),
        
        ViskInstruction.PushConstD(4),
        ViskInstruction.PushConstD(5),
        ViskInstruction.PushConstD(6),
        ViskInstruction.PushConstD(7),
        ViskInstruction.PushConstD(8),
        ViskInstruction.PushConstD(9),
        ViskInstruction.CallForeign(printDoubles),

        ViskInstruction.PushConst(0),
        ViskInstruction.Ret()
    }
);

var image = new ViskImage(module);
var executor = image.Compile(new ViskSettings(CompilationMode.Debug));
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