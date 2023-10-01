using Visk;
using ViskCompiler;

var printLongs = typeof(Helper).GetMethod(nameof(Helper.PrintLongs));
var printLong = typeof(Helper).GetMethod(nameof(Helper.PrintLong));
var printSmt = typeof(Helper).GetMethod(nameof(Helper.PrintSmt));

var module = new ViskModule("main");

var fMain = module.AddFunction("main", 0, false);
var fOther = module.AddFunction("otherFunc", 0, false);

fMain.Instructions.AddRange(new List<ViskInstruction>
{
    ViskInstruction.Call(fOther),
    ViskInstruction.CallForeign(printSmt),
    ViskInstruction.Call(fOther),
    ViskInstruction.CallForeign(printSmt),
    ViskInstruction.Call(fOther),

    ViskInstruction.PushConst(123),
    ViskInstruction.Ret()
});

fOther.Instructions.AddRange(new List<ViskInstruction>
{
    ViskInstruction.PushConst(120),
    ViskInstruction.CallForeign(printLong),
    ViskInstruction.Ret()
});


var image = new ViskImage(module);
var executor = image.Compile();
var asmDelegate = executor.GetDelegate();

Console.WriteLine(new string('-', Console.WindowWidth));
Console.WriteLine(executor.ToString());
Console.WriteLine(new string('-', Console.WindowWidth));

for (int i = 0; i < 10; i++)
{
    Console.WriteLine($"Function returned: {asmDelegate()}");
    Console.WriteLine("\n\n");
}