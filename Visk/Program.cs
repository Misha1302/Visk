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

    ViskInstruction.PushConst(123),
    ViskInstruction.Ret()
});

fOther.Instructions.AddRange(new List<ViskInstruction>
{
    ViskInstruction.PushConst(20),
    ViskInstruction.PushConst(30),
    ViskInstruction.PushConst(40),
    ViskInstruction.PushConst(50),
    ViskInstruction.PushConst(60),
    ViskInstruction.PushConst(70),
    ViskInstruction.PushConst(80),
    ViskInstruction.PushConst(90),
    ViskInstruction.PushConst(100),
    ViskInstruction.PushConst(100),
    ViskInstruction.CallForeign(printLongs),
    
    ViskInstruction.PushConst(20),
    ViskInstruction.PushConst(30),
    ViskInstruction.PushConst(40),
    ViskInstruction.PushConst(50),
    ViskInstruction.PushConst(60),
    ViskInstruction.PushConst(70),
    ViskInstruction.PushConst(80),
    ViskInstruction.PushConst(90),
    ViskInstruction.PushConst(100),
    ViskInstruction.PushConst(100),
    ViskInstruction.CallForeign(printLongs),
    
    
    ViskInstruction.Nop(),
    ViskInstruction.PushConst(-999999999),
    ViskInstruction.CallForeign(printLong),
    ViskInstruction.Nop(),
    
    ViskInstruction.Ret()
});


var image = new ViskImage(module);
var executor = image.Compile();
var asmDelegate = executor.GetDelegate();

Console.WriteLine(new string('-', Console.WindowWidth));
Console.WriteLine(executor.ToString());
Console.WriteLine(new string('-', Console.WindowWidth));

for (int i = 0; i < 1; i++)
{
    Console.WriteLine($"Function returned: {asmDelegate()}");
    Console.WriteLine("\n\n");
}