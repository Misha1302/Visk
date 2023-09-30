using Visk;
using ViskCompiler;

var printLongs = typeof(Helper).GetMethod(nameof(Helper.PrintLongs));
var printLong = typeof(Helper).GetMethod(nameof(Helper.PrintLong));

var module = new ViskModule("main");

module.AddFunction("f").Instructions.AddRange(new List<ViskInstruction>
{
    ViskInstruction.PushConst(10),
    ViskInstruction.PushConst(20),
    ViskInstruction.PushConst(30),
    ViskInstruction.PushConst(40),
    ViskInstruction.PushConst(50),
    ViskInstruction.PushConst(60),
    ViskInstruction.PushConst(70),
    ViskInstruction.PushConst(80),
    ViskInstruction.PushConst(90),
    ViskInstruction.PushConst(100),
    ViskInstruction.PushConst(110),
    ViskInstruction.PushConst(120),
    ViskInstruction.Add(),
    ViskInstruction.Add(),
    ViskInstruction.Add(),
    ViskInstruction.Add(),
    ViskInstruction.Add(),
    ViskInstruction.Add(),
    ViskInstruction.Add(),
    ViskInstruction.Add(),
    ViskInstruction.Add(),
    ViskInstruction.Add(),
    ViskInstruction.Add(),
    ViskInstruction.CallForeign(printLong),

    ViskInstruction.PushConst(120),
    ViskInstruction.CallForeign(printLong),


    ViskInstruction.PushConst(10),
    ViskInstruction.PushConst(20),
    ViskInstruction.PushConst(30),
    ViskInstruction.PushConst(40),
    ViskInstruction.PushConst(50),
    ViskInstruction.PushConst(60),
    ViskInstruction.PushConst(70),
    ViskInstruction.PushConst(80),
    ViskInstruction.PushConst(90),
    ViskInstruction.PushConst(100),
    ViskInstruction.CallForeign(printLongs),

    ViskInstruction.PushConst(123),
    ViskInstruction.Ret()
});


var image = new ViskImage(module);
var executor = image.Compile();
var asmDelegate = executor.GetDelegate();

Console.WriteLine(new string('-', Console.WindowWidth));
Console.WriteLine(executor.ToString());
Console.WriteLine(new string('-', Console.WindowWidth));

Console.WriteLine($"Function returned: {asmDelegate()}");