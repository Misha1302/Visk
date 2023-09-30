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
    ViskInstruction.Add(),
    ViskInstruction.Add(),
    ViskInstruction.Add(),
    ViskInstruction.Add(),
    ViskInstruction.Add(),
    
    ViskInstruction.Ret()
});


var image = new ViskImage(module);
var executor = image.Compile();
var asmDelegate = executor.GetDelegate();

Console.WriteLine(new string('-', Console.WindowWidth));
Console.WriteLine(executor.ToString());
Console.WriteLine(new string('-', Console.WindowWidth));

Console.WriteLine($"Function returned: {asmDelegate()}");