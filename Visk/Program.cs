using System.Diagnostics;
using Visk;
using ViskCompiler;

var printLongs = typeof(Helper).GetMethod(nameof(Helper.PrintLongs));
var printLong = typeof(Helper).GetMethod(nameof(Helper.PrintLong));
var printSmt = typeof(Helper).GetMethod(nameof(Helper.PrintSmt));

var module = new ViskModule("main");

var fMain = module.AddFunction("main", 0, typeof(long));
// var fOther = module.AddFunction("other", 8, typeof(long));

fMain.RawInstructions.AddRange(new List<ViskInstruction>
{
    ViskInstruction.PushConst(0),
    ViskInstruction.SetLocal("l"),

    ViskInstruction.SetLabel("hi"),

    ViskInstruction.LoadLocal("l"),
    ViskInstruction.PushConst(1),
    ViskInstruction.Add(),
    ViskInstruction.SetLocal("l"),


    ViskInstruction.LoadLocal("l"),
    ViskInstruction.PushConst(10),
    ViskInstruction.Cmp(),

    ViskInstruction.GotoIfNotEquals("hi"),

    ViskInstruction.LoadLocal("l"),
    ViskInstruction.Ret()
});


var image = new ViskImage(module);
image.Compile();
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