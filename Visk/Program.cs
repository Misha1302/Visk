using ViskCompiler;

var compiler = new ViskCompiler.ViskCompiler(new List<ViskInstruction>
{
    ViskInstruction.PushConst(2),
    ViskInstruction.PushConst(2),
    ViskInstruction.CallForeign(typeof(Helper).GetMethod(nameof(Helper.GetFour)), 0, ViskType.Int64),
    ViskInstruction.Add(),
    ViskInstruction.IMul(),
    ViskInstruction.CSharpRet()
});

var executor = compiler.Compile();
var asmDelegate = executor.GetDelegate();
Console.WriteLine(executor.ToString());
Console.WriteLine(asmDelegate());

public static class Helper
{
    public static long GetFour() => (long)(23.4f / GetFive());
    private static long GetFive() => 5;
}