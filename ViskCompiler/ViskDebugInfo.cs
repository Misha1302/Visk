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
        var index = _instructions.BinarySearch(x => x.AssemblerInstructionIndex - assemblerIndex);
        return index >= 0 ? _instructions[index] : null;
    }

    public ViskFunctionInfo? GetFunction(int assemblerIndex)
    {
        var index = _functions.BinarySearch(x => x.AssemblerInstructionIndex - assemblerIndex);
        return index >= 0 ? _functions[index] : null;
    }
}