namespace ViskCompiler;

public sealed class ViskDebugInfo
{
    public readonly List<ViskInstruction> Instructions = new();
    public readonly List<ViskFunctionInfo> Functions = new();
}