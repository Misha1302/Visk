namespace ViskCompiler;

using Iced.Intel;

internal sealed class ViskLocals
{
    private static readonly AssemblerRegister64[] _registersForLocals = { AssemblerRegisters.r15 };

    private readonly Dictionary<string, ViskT2<AssemblerRegister64?, AssemblerMemoryOperand?>> _locals = new();

    public ViskLocals(IReadOnlyList<string> locals, ViskFunctionStackManager funcStackManager)
    {
        var min = Math.Min(_registersForLocals.Length, locals.Count);
        for (var index = 0; index < min; index++)
        {
            var value = new ViskT2<AssemblerRegister64?, AssemblerMemoryOperand?>(_registersForLocals[index]);
            _locals.Add(locals[index], value);
        }

        for (var index = min; index < locals.Count; index++)
        {
            var value = new ViskT2<AssemblerRegister64?, AssemblerMemoryOperand?>(
                funcStackManager.GetMemoryLocal((index + 1) * 8)
            );
            _locals.Add(locals[index], value);
        }
    }

    public int Size => _locals.Count * ViskStack.BlockSize + ViskStack.BlockSize;

    public bool GetLocalPos(string name, out AssemblerRegister64? regPos, out AssemblerMemoryOperand? memPos)
    {
        regPos = _locals[name].Value0;
        memPos = _locals[name].Value1;
        return regPos != null;
    }
}