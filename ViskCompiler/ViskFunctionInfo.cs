namespace ViskCompiler;

public sealed class ViskFunctionInfo : IViskAssemblerPositionable
{
    public readonly string Name;
    public readonly int ArgsCount;
    public readonly Type ReturnType;
    public readonly bool IsMain;

    public ViskFunctionInfo(string name, int argsCount, Type returnType, bool isMain)
    {
        Name = name;
        ArgsCount = argsCount;
        ReturnType = returnType;
        IsMain = isMain;
    }

    public int AssemblerInstructionIndex { get; set; }

    public override string ToString() => $"{Name} {ArgsCount} {ReturnType} {IsMain}";
}