using Visk;
using ViskCompiler;


var printLongMultipliedByHalf = typeof(Helper).GetMethod(nameof(Helper.PrintLongs));

var compiler = new ViskCompiler.ViskCompiler(new List<ViskInstruction>
{
    ViskInstruction.Prolog(0),

    ViskInstruction.SetLabel("label"),
    
    ViskInstruction.PushConst(123), // 123
    ViskInstruction.PushConst(124), // 123, 124
    ViskInstruction.PushConst(125), // 123. 124, 125
    ViskInstruction.PushConst(126), // 123. 124, 125, 126
    ViskInstruction.PushConst(127), // 123. 124, 125, 126, 127
    ViskInstruction.PushConst(128), // 123. 124, 125, 126, 127, 128
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