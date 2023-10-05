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

    public void ForeignMoveArgs(int argsCount)
    {
        var regOfOffset = new RegOrOffset(_dataManager.Stack, _dataManager.Register);

        if (argsCount >= _argsRegisters.Length)
        {
            var size = (argsCount - _argsRegisters.Length) * 8;
            var maxSize = 32 + size;

            var delta = maxSize + (ViskStack.PosStackAlign - maxSize % ViskStack.PosStackAlign);
            _stackChanged += delta;
            if (delta != 0)
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

    public void MoveArgs(int argsCount)
    {
        var regOfOffset = new RegOrOffset(_dataManager.Stack, _dataManager.Register);

        var totalSize = argsCount * 8 + argsCount * 8 % 16;
        _dataManager.Assembler.sub(rsp, totalSize);
        var pointer = totalSize - 8;

        for (var i = 0; i < argsCount; i++, pointer -= 8)
            if (regOfOffset.GetRegisterOrOffset(out var r, out var offset))
            {
                _dataManager.Assembler.mov(__[rsp + pointer], r!.Value);
            }
            else if (r is null && offset is null)
            {
                ThrowHelper.ThrowInvalidOperationException();
            }
            else
            {
                _dataManager.Assembler.mov(rax, offset!.Value);

                _dataManager.Assembler.mov(__[rsp + pointer], rax);
            }
    }

    public void SaveRegs()
    {
        if (_stackChanged != 0)
            ThrowHelper.ThrowInvalidOperationException("You must to move args after call this method");

        for (var index = 0; index < _dataManager.Register.CurIndex; index++)
        {
            var register = ViskRegister.Registers[index];
            _dataManager.Assembler.mov(_dataManager.FuncStackManager.GetMemoryToSaveReg(index), register);
        }

        _regsCount = _dataManager.Register.CurIndex;
    }

    public void LoadRegs()
    {
        if (_stackChanged != 0)
            _dataManager.Assembler.add(rsp, _stackChanged);

        for (var index = _regsCount - 1; index >= 0; index--)
        {
            var register = ViskRegister.Registers[index];
            _dataManager.Assembler.mov(register, _dataManager.FuncStackManager.GetMemoryToSaveReg(index));
        }

        _stackChanged = 0;
    }

    public void SaveReturnValue(Type rt)
    {
        if (rt == typeof(void)) return;

        if (rt != typeof(long))
            ThrowHelper.ThrowInvalidOperationException("Unknown return type");

        if (_dataManager.Register.CanGetNext)
            _dataManager.Assembler.mov(_dataManager.Register.Next(), rax);
        else _dataManager.Assembler.mov(_dataManager.Stack.GetNext(), rax);
    }
}