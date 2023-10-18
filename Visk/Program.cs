using System.Diagnostics;
using Visk;
using ViskCompiler;

// ReSharper disable UnusedVariable
var printLongs = typeof(Helper).GetMethod(nameof(Helper.PrintLongs));
var printLong = typeof(Helper).GetMethod(nameof(Helper.PrintLong));
var inputLong = typeof(Helper).GetMethod(nameof(Helper.InputLong));
var inputDouble = typeof(Helper).GetMethod(nameof(Helper.InputDouble));
var printDouble = typeof(Helper).GetMethod(nameof(Helper.PrintDouble));
var printDoubles = typeof(Helper).GetMethod(nameof(Helper.PrintDoubles));
var printSmt = typeof(Helper).GetMethod(nameof(Helper.PrintSmt));
var printByPointer = typeof(Helper).GetMethod(nameof(Helper.PrintByPointer));

var module = new ViskModule("main");
var mf = module.AddFunction("main", new List<Type>(0), ViskConsts.I64);
var of = module.AddFunction("other", new List<Type> { ViskConsts.I64 }, ViskConsts.None);

mf.AddInstructions(new List<ViskInstruction>
    {
        ViskInstruction.PushConst(0),
        ViskInstruction.SetLocal("someChar"),
        
        ViskInstruction.SetLabel("start"),
        
        
        ViskInstruction.PushConst(123),
        ViskInstruction.CallBuildIn(ViskBuildIn.Alloc),
        ViskInstruction.SetLocal("pointer"),

        ViskInstruction.PushConst(4), // len of str
        ViskInstruction.LoadLocal("pointer"),
        ViskInstruction.PushConst(0),
        ViskInstruction.Add(),
        ViskInstruction.SetByRef(sizeof(long)),


        ViskInstruction.PushConst('К'),
        ViskInstruction.LoadLocal("pointer"),
        ViskInstruction.PushConst(8),
        ViskInstruction.Add(),
        ViskInstruction.SetByRef(sizeof(char)),
        
        ViskInstruction.PushConst('у'),
        ViskInstruction.LoadLocal("pointer"),
        ViskInstruction.PushConst(10),
        ViskInstruction.Add(),
        ViskInstruction.SetByRef(sizeof(char)),

        ViskInstruction.LoadLocal("someChar"),
        ViskInstruction.LoadLocal("pointer"),
        ViskInstruction.PushConst(12),
        ViskInstruction.Add(),
        ViskInstruction.SetByRef(sizeof(char)),

        ViskInstruction.PushConst('\n'),
        ViskInstruction.LoadLocal("pointer"),
        ViskInstruction.PushConst(14),
        ViskInstruction.Add(),
        ViskInstruction.SetByRef(sizeof(char)),

        ViskInstruction.LoadLocal("pointer"),
        ViskInstruction.CallForeign(printByPointer),


        ViskInstruction.LoadLocal("pointer"),
        ViskInstruction.CallBuildIn(ViskBuildIn.Free),
        
        ViskInstruction.LoadLocal("someChar"),
        ViskInstruction.PushConst(1),
        ViskInstruction.Add(),
        ViskInstruction.SetLocal("someChar"),
        
        ViskInstruction.LoadLocal("someChar"),
        ViskInstruction.PushConst(1150),
        ViskInstruction.LessThan(),
        ViskInstruction.IfTrue(ViskInstruction.Goto("start")),

        ViskInstruction.PushConst(0),
        ViskInstruction.Ret()
    }
);

var image = new ViskImage(module);
var executor = image.Compile(new ViskSettings(ViskCompilationMode.Debug));
var asmDelegate = executor.GetDelegate();

var assemblerString = executor.ToString();
Console.WriteLine(new string('-', Console.WindowWidth));
Console.WriteLine(assemblerString);
Console.WriteLine(new string('-', Console.WindowWidth));
assemblerString.SaveFileAsync("debugInfo.txt");

var sw = new Stopwatch();
for (var i = 0; i < 5; i++)
{
    sw.Restart();

    Console.WriteLine($"Function returned: {asmDelegate()}; Approximate execution time: {sw.ElapsedMilliseconds}\n\n");
}