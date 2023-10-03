using Visk;
using ViskCompiler;

var printLongs = typeof(Helper).GetMethod(nameof(Helper.PrintLongs));
var printLong = typeof(Helper).GetMethod(nameof(Helper.PrintLong));
var printSmt = typeof(Helper).GetMethod(nameof(Helper.PrintSmt));

var module = new ViskModule("main");

var fMain = module.AddFunction("main", 0, false);
var fOther = module.AddFunction("otherFunc", 0, false);

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

    ViskInstruction.CallForeign(printLongs),

    ViskInstruction.PushConst(11),
    ViskInstruction.CallForeign(printLong),

    ViskInstruction.Ret()
});

fOther.RawInstructions.AddRange(new List<ViskInstruction>
{
    ViskInstruction.PushConst(100),

    //ViskInstruction.CallForeign(printLong),
    ViskInstruction.Ret()
});


var image = new ViskImage(module);
var executor = image.Compile();
var asmDelegate = executor.GetDelegate();

Console.WriteLine(new string('-', Console.WindowWidth));
Console.WriteLine(executor.ToString());
Console.WriteLine(new string('-', Console.WindowWidth));

for (var i = 0; i < 1; i++)
{
    Console.WriteLine($"Function returned: {asmDelegate()}");
    Console.WriteLine("\n\n");
}