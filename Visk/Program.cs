using Visk;
using ViskCompiler;

var printLong = typeof(Helper).GetMethod(nameof(Helper.PrintLong));

var module = new ViskModule("main");

module.AddFunction("f").AddRange(new List<ViskInstruction>
{
    ViskInstruction.PushConst(0),
    ViskInstruction.SetLocal("i"),

    ViskInstruction.SetLabel("label"),

    ViskInstruction.LoadLocal("i"),
    ViskInstruction.CallForeign(printLong),

    ViskInstruction.LoadLocal("i"),
    ViskInstruction.PushConst(1),
    ViskInstruction.Add(),
    ViskInstruction.SetLocal("i"),

    ViskInstruction.Goto("label"),

    ViskInstruction.PushConst(0), // 0
    ViskInstruction.Ret()
});


var image = new ViskImage(module);
var executor = image.Compile();
var asmDelegate = executor.GetDelegate();

Console.WriteLine(new string('-', Console.WindowWidth));
Console.WriteLine(executor.ToString());
Console.WriteLine(new string('-', Console.WindowWidth));

Console.WriteLine($"Function returned: {asmDelegate()}");