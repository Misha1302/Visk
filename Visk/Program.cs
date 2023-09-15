using Visk;
using ViskCompiler;

var printLongMultipliedByHalf = typeof(Helper).GetMethod(nameof(Helper.PrintLongMultipliedByHalf));

var compiler = new ViskCompiler.ViskCompiler(new List<ViskInstruction>
{
    ViskInstruction.Prolog(0),
    
    ViskInstruction.SetLabel("label"),
    ViskInstruction.PushConst(123), // 123
    ViskInstruction.PushConst(123), // 123, 123
    ViskInstruction.PushConst(123), // 123. 123, 123
    ViskInstruction.CallForeign(printLongMultipliedByHalf),

    ViskInstruction.PushConst(2), // 123, 123, 2
    ViskInstruction.Add(), // 123, 125
    ViskInstruction.Add(), // 248
    ViskInstruction.CallForeign(printLongMultipliedByHalf),
    ViskInstruction.Goto("label"),

    ViskInstruction.PushConst(0), // 0
    ViskInstruction.Ret()
});

var executor = compiler.Compile();
var asmDelegate = executor.GetDelegate();

Console.WriteLine(new string('-', Console.WindowWidth));
Console.WriteLine(executor.ToString());
Console.WriteLine(new string('-', Console.WindowWidth));

Console.WriteLine($"Function returned: {asmDelegate()}");