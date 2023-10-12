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
        ViskInstruction.PushConstD(123.321),
        ViskInstruction.SetLocalD("i"),
        
        
        ViskInstruction.LoadLocalD("i"),
        ViskInstruction.CallForeign(printDouble),

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