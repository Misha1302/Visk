namespace ViskCompiler;

using Iced.Intel;

internal sealed class ViskLocals
{
    public readonly List<(string name, ViskT2<AssemblerRegister64?, AssemblerMemoryOperand?> pos)> Locals = new();

    public ViskLocals(IReadOnlyList<string> locals, ViskFunctionStackManager funcStackManager)
    {
        var min = Math.Min(ViskRegister.LocalsRegisters.Length, locals.Count);
        for (var index = 0; index < min; index++)
        {
            var value = new ViskT2<AssemblerRegister64?, AssemblerMemoryOperand?>(ViskRegister.LocalsRegisters[index]);
            Locals.Add((locals[index], value));
        }

        for (var index = min; index < locals.Count; index++)
        {
            var value = new ViskT2<AssemblerRegister64?, AssemblerMemoryOperand?>(
                funcStackManager.GetMemoryLocal((index + 1) * 8)
            );
            Locals.Add((locals[index], value));
        }
    }

    public int Size => Locals.Count * ViskStack.BlockSize + ViskStack.BlockSize;

    public bool GetLocalPos(string name, out AssemblerRegister64? regPos, out AssemblerMemoryOperand? memPos)
    {
        regPos = Locals.FirstOrDefault(x => x.name == name).pos?.Value0;
        memPos = Locals.FirstOrDefault(x => x.name == name).pos?.Value1;
        return regPos != null;
    }
}