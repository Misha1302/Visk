namespace ViskCompiler;

public interface IViskDebugInfo
{
    public void AddInstruction(ViskInstruction instruction);
    public void AddFunction(ViskFunctionInfo function);

    public ViskInstruction? GetInstruction(int assemblerIndex);
    public ViskFunctionInfo? GetFunction(int assemblerIndex);
}