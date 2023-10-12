namespace ViskCompiler;

internal sealed class ViskDebugInfoPlug : IViskDebugInfo
{
    public void AddInstruction(ViskInstruction instruction)
    {
    }

    public void AddFunction(ViskFunctionInfo function)
    {
    }

    public ViskInstruction? GetInstruction(int assemblerIndex) => null;

    public ViskFunctionInfo? GetFunction(int assemblerIndex) => null;
}