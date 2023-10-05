using System.Diagnostics;
using Visk;
using ViskCompiler;

var printLongs = typeof(Helper).GetMethod(nameof(Helper.PrintLongs));
var printLong = typeof(Helper).GetMethod(nameof(Helper.PrintLong));
var printSmt = typeof(Helper).GetMethod(nameof(Helper.PrintSmt));

var module = new ViskModule("main");

var fMain = module.AddFunction("main", 0, typeof(long));
var fOther = module.AddFunction("other", 0, typeof(long));

fMain.RawInstructions.AddRange(new List<ViskInstruction>
{
    ViskInstruction.Nop(),
    ViskInstruction.Nop(),
    ViskInstruction.Nop(),
    
    ViskInstruction.Call(fOther),
    ViskInstruction.Nop(),

    ViskInstruction.Call(fOther),
    ViskInstruction.Nop(),

    ViskInstruction.Call(fOther),
    ViskInstruction.Nop(),

    ViskInstruction.Add(),
    ViskInstruction.Nop(),

    ViskInstruction.Add(),
    ViskInstruction.Nop(),

    ViskInstruction.Ret(), ViskInstruction.Nop(), ViskInstruction.Nop()
});

fOther.RawInstructions.AddRange(new List<ViskInstruction>
{
    ViskInstruction.PushConst(0),
    ViskInstruction.SetLocal("loc"),

    ViskInstruction.SetLabel("Label"),

    // ViskInstruction.LoadLocal("loc"),
    // ViskInstruction.CallForeign(printLong),

    ViskInstruction.LoadLocal("loc"),
    ViskInstruction.PushConst(1),
    ViskInstruction.Add(),
    ViskInstruction.SetLocal("loc"),

    ViskInstruction.LoadLocal("loc"),
    ViskInstruction.PushConst(111_111),
    ViskInstruction.Cmp(),
    ViskInstruction.GotoIfNotEquals("Label"),

    ViskInstruction.LoadLocal("loc"),
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