namespace ViskCompiler;

public sealed class ViskFunctionInfo : IViskAssemblerPositionable
{
    public readonly string Name;
    public readonly List<Type> Params;
    public readonly Type ReturnType;
    public readonly bool IsMain;

    public ViskFunctionInfo(string name, List<Type> parameters, Type returnType, bool isMain)
    {
        Name = name;
        Params = parameters;
        ReturnType = returnType;
        IsMain = isMain;
    }

    public int AssemblerInstructionIndex { get; set; }

    public override string ToString() => $"{Name} {Params} {ReturnType} {IsMain}";
}