namespace ViskCompiler;

using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal static class ArgsManager
{
    private static readonly AssemblerRegister64[] _assemblerRegisters = { rcx, rdx, r8, r9 };

    public static void MoveArgsToRegisters(int argsCount, ViskRegister fromReg, Assembler assembler)
    {
        if (argsCount >= _assemblerRegisters.Length)
            throw new InvalidOperationException();

        var reg = new ViskRegister(fromReg.CurValue);

        for (var i = 0; i < argsCount; i++) 
            assembler.mov(_assemblerRegisters[i], reg.Previous());
    }
}