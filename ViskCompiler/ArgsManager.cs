namespace ViskCompiler;

using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal static class ArgsManager
{
    private static readonly AssemblerRegister64[] _assemblerRegisters = { rcx, rdx, r8, r9 };

    public static void MoveArgs(int argsCount, ViskRegister fromReg, Assembler assembler,
        Stack<AssemblerMemoryOperand> dataInStack, out bool stackAligned, out int registerUsed)
    {
        registerUsed = 0;
        stackAligned = false;

        var regOfOffset = new RegOrOffset(dataInStack, new ViskRegister(fromReg.CurIndex));

        if (argsCount >= _assemblerRegisters.Length)
        {
            stackAligned = true;
            var size = (argsCount - _assemblerRegisters.Length) * 8;
            var maxSize = 32 + size;

            assembler.sub(rsp, maxSize);
            assembler.and(sp, ViskCompiler.StackAlignConst);

            var i = maxSize - 8;
            for (var j = 0; j < argsCount - _assemblerRegisters.Length; j++, i -= 8)
                if (regOfOffset.GetRegisterOrOffset(out var r, out var offset))
                {
                    registerUsed++;
                    assembler.mov(__[rsp + i], r!.Value);
                }
                else if (r is null && offset is null)
                {
                    throw new InvalidOperationException();
                }
                else
                {
                    assembler.mov(rax, offset!.Value);
                    assembler.mov(__[rsp + i], rax);
                }
        }

        var min = Math.Min(argsCount, _assemblerRegisters.Length);
        for (var j = min - 1; j >= 0; j--)
            if (regOfOffset.GetRegisterOrOffset(out var r, out var offset))
            {
                registerUsed++;
                assembler.mov(_assemblerRegisters[j], r!.Value);
            }
            else if (r is null && offset is null)
            {
                throw new InvalidOperationException();
            }
            else
            {
                assembler.mov(_assemblerRegisters[j], offset!.Value);
            }
    }

    private sealed class RegOrOffset
    {
        private readonly Stack<AssemblerMemoryOperand> _dataInStack;
        private readonly ViskRegister _register;

        public RegOrOffset(Stack<AssemblerMemoryOperand> dataInStack, ViskRegister register)
        {
            _dataInStack = dataInStack;
            _register = register;
        }

        public bool GetRegisterOrOffset(out AssemblerRegister64? register64, out AssemblerMemoryOperand? offset) =>
            StackAtFirst(out register64, out offset);

        private bool StackAtFirst(out AssemblerRegister64? register64, out AssemblerMemoryOperand? offset)
        {
            if (_dataInStack.Count != 0)
            {
                offset = _dataInStack.Pop();
                register64 = null;
                return false;
            }

            offset = null;
            register64 = _register.Previous();
            return true;
        }
    }
}