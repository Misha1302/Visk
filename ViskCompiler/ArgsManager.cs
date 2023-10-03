namespace ViskCompiler;

using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal sealed class ArgsManager
{
    private static readonly AssemblerRegister64[] _argsRegisters = { rcx, rdx, r8, r9 };
    private readonly ViskDataManager _dataManager;
    private int _stackChanged;
    private int _regsCount;

    public ArgsManager(ViskDataManager dataManager)
    {
        _dataManager = dataManager;
    }

    public void MoveArgs(int argsCount)
    {
        var regOfOffset = new RegOrOffset(_dataManager.Stack, _dataManager.Register);

        if (argsCount >= _argsRegisters.Length)
        {
            var size = (argsCount - _argsRegisters.Length) * 8;
            var maxSize = 32 + size;

            var delta = maxSize + (ViskStack.PosStackAlign - maxSize % ViskStack.PosStackAlign);
            _stackChanged += delta;
            _dataManager.Assembler.sub(rsp, delta);

            var i = maxSize - 8;
            for (var j = 0; j < argsCount - _argsRegisters.Length; j++, i -= 8)
                if (regOfOffset.GetRegisterOrOffset(out var r, out var offset))
                {
                    _dataManager.Assembler.mov(__[rsp + i], r!.Value);
                }
                else if (r is null && offset is null)
                {
                    ThrowHelper.ThrowInvalidOperationException();
                }
                else
                {
                    _dataManager.Assembler.mov(rax, offset!.Value);
                    _dataManager.Assembler.mov(__[rsp + i], rax);
                }
        }

        var min = Math.Min(argsCount, _argsRegisters.Length);
        for (var j = min - 1; j >= 0; j--)
            if (regOfOffset.GetRegisterOrOffset(out var r, out var offset))
                _dataManager.Assembler.mov(_argsRegisters[j], r!.Value);
            else if (r is null && offset is null)
                ThrowHelper.ThrowInvalidOperationException();
            else
                _dataManager.Assembler.mov(_argsRegisters[j], offset!.Value);
    }

    public void SaveRegs()
    {
        if (_stackChanged != 0)
            ThrowHelper.ThrowInvalidOperationException("You must to move args after call this method");

        for (var index = 0; index < _dataManager.Register.CurIndex; index++)
        {
            var register = ViskRegister.Registers[index];
            _dataManager.Assembler.mov(GetMemOp(index), register);
        }

        _regsCount = _dataManager.Register.CurIndex;
    }

    private AssemblerMemoryOperand GetMemOp(int index) => __[rbp - _dataManager.CurrentFuncMaxStackSize - (index + 1) * ViskStack.BlockSize];

    public void LoadRegs()
    {
        _dataManager.Assembler.add(rsp, _stackChanged);

        for (var index = _regsCount - 1; index >= 0; index--)
        {
            var register = ViskRegister.Registers[index];
            _dataManager.Assembler.mov(register, GetMemOp(index));
        }

        _stackChanged = 0;
    }
}