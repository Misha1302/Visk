namespace ViskCompiler;

public sealed class ViskDebugInfo : IViskDebugInfo
{
    private readonly List<ViskInstruction> _instructions = new();
    private readonly List<ViskFunctionInfo> _functions = new();

    public void AddInstruction(ViskInstruction instruction)
    {
        _instructions.Add(instruction);
    }

    public void AddFunction(ViskFunctionInfo function)
    {
        _functions.Add(function);
    }

    public ViskInstruction? GetInstruction(int assemblerIndex)
    {
        return _instructions.Find(x => x.AssemblerInstructionIndex == assemblerIndex);
    }

    public ViskFunctionInfo? GetFunction(int assemblerIndex)
    {
        return _functions.Find(x => x.AssemblerInstructionIndex == assemblerIndex);
    }
}