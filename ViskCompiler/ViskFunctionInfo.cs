namespace ViskCompiler;

public sealed class ViskFunctionInfo : IAssemblerPositionable
{
    public readonly string Name;
    public readonly int ArgsCount;
    public readonly Type ReturnType;
    public readonly bool IsMain;
    public int AssemblerInstructionIndex { get; set; }

    public ViskFunctionInfo(string name, int argsCount, Type returnType, bool isMain)
    {
        Name = name;
        ArgsCount = argsCount;
        ReturnType = returnType;
        IsMain = isMain;
    }
}