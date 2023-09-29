namespace ViskCompiler;

using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal static class ArgsManager
{
    private static readonly AssemblerRegister64[] _assemblerRegisters = { rcx, rdx, r8, r9 };

    public static void MoveArgs(int argsCount, ViskRegister fromReg, Assembler assembler, List<int> dataInStack,
        out bool stackAligned)
    {
        stackAligned = false;


        var regOfOffset = new RegOrOffset(dataInStack, new ViskRegister(fromReg.CurValue));

        if (argsCount >= _assemblerRegisters.Length)
        {
            stackAligned = true;
            var size = (argsCount - _assemblerRegisters.Length) * 8;
            var maxSize = 32 + size;

            assembler.sub(rsp, maxSize);
            assembler.and(sp, -0xF);

            var i = maxSize - 8;
            for (var j = 0; j < argsCount - _assemblerRegisters.Length; j++, i -= 8)
                if (regOfOffset.GetRegisterOrOffset(out var r, out var offset))
                {
                    assembler.mov(__[rsp + i], r!.Value);
                }
                else if (r is null && offset is null)
                {
                    throw new InvalidOperationException();
                }
                else
                {
                    assembler.mov(rax, __[rbp - offset!.Value]);
                    assembler.mov(__[rsp + i], rax);
                }
        }

        var min = Math.Min(argsCount, _assemblerRegisters.Length);
        for (var j = min - 1; j >= 0; j--)
            if (regOfOffset.GetRegisterOrOffset(out var r, out var offset))
                assembler.mov(_assemblerRegisters[j], r!.Value);
            else if (r is null && offset is null)
                throw new InvalidOperationException();
            else assembler.mov(_assemblerRegisters[j], __[rbp - offset!.Value]);
    }

    private sealed class RegOrOffset
    {
        private readonly List<int> _dataInStack;
        private readonly ViskRegister _register;
        private int _pointer;

        public RegOrOffset(List<int> dataInStack, ViskRegister register)
        {
            _dataInStack = dataInStack;
            _register = register;
            _pointer = 0;
        }

        public bool GetRegisterOrOffset(out AssemblerRegister64? register64, out int? offset) =>
            StackAtFirst(out register64, out offset);

        private bool StackAtFirst(out AssemblerRegister64? register64, out int? offset)
        {
            if (_pointer < _dataInStack.Count)
            {
                offset = _dataInStack[^++_pointer];
                register64 = null;
                return false;
            }

            offset = null;
            register64 = _register.Previous();
            return true;
        }
    }
}