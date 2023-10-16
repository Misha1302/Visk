namespace ViskCompiler;

internal sealed class ViskLocals
{
    public readonly
        List<(string name, ViskT3<AssemblerRegister64?, AssemblerRegisterXMM?, AssemblerMemoryOperand?> pos)> Locals =
            new();

    public ViskLocals(IReadOnlyList<ViskLocal> locals, ViskFunctionStackManager funcStackManager)
    {
        var min = Math.Min(ViskLocal.LocalsRegisters.Length, locals.Count);
        for (var index = 0; index < min; index++)
        {
            var value = locals[index].Type == ViskConsts.I64
                ? new ViskT3<AssemblerRegister64?, AssemblerRegisterXMM?, AssemblerMemoryOperand?>(
                    ViskLocal.LocalsRegisters[index])
                : new ViskT3<AssemblerRegister64?, AssemblerRegisterXMM?, AssemblerMemoryOperand?>(
                    ViskLocal.LocalsRegistersD[index]);

            Locals.Add((locals[index].Name, value));
        }

        for (var index = min; index < locals.Count; index++)
        {
            var value = new ViskT3<AssemblerRegister64?, AssemblerRegisterXMM?, AssemblerMemoryOperand?>(
                funcStackManager.GetMemoryLocal((index + 1) * 8)
            );
            Locals.Add((locals[index].Name, value));
        }
    }

    public int Size => Locals.Count * ViskStack.BlockSize + ViskStack.BlockSize;

    public bool GetLocalPos(string name, out AssemblerRegister64? regPos, out AssemblerRegisterXMM? xmmPos,
        out AssemblerMemoryOperand? memPos)
    {
        regPos = Locals.FirstOrDefault(x => x.name == name).pos?.Value0;
        xmmPos = Locals.FirstOrDefault(x => x.name == name).pos?.Value1;
        memPos = Locals.FirstOrDefault(x => x.name == name).pos?.Value2;
        return regPos != null || xmmPos != null;
    }
}