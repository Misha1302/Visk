namespace ViskCompiler;

using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal sealed class ViskArgsManager
{
    private static readonly AssemblerRegister64[] _argsRegisters = { rcx, rdx, r8, r9 };
    private readonly ViskDataManager _dataManager;
    private int _stackChanged;
    private int _regsCount;
    private int _localsCount;
    private int _index;

    public ViskArgsManager(ViskDataManager dataManager)
    {
        _dataManager = dataManager;
    }

    public void ForeignMoveArgs(int argsCount)
    {
        var regOfOffset = new ViskRegOrOffset(_dataManager.Stack, _dataManager.Register);

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
                    ViskThrowHelper.ThrowInvalidOperationException();
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
                ViskThrowHelper.ThrowInvalidOperationException();
            else
                _dataManager.Assembler.mov(_argsRegisters[j], offset!.Value);
    }

    // it's stdcall i guess?
    public void MoveArgs(int argsCount)
    {
        var regOfOffset = new ViskRegOrOffset(_dataManager.Stack, _dataManager.Register);

        var totalSize = argsCount * 8 + argsCount * 8 % 16;
        _dataManager.Assembler.sub(rsp, totalSize);
        var pointer = argsCount * 8 - 8;

        for (var i = 0; i < argsCount; i++, pointer -= 8)
            if (regOfOffset.GetRegisterOrOffset(out var r, out var offset))
            {
                _dataManager.Assembler.mov(__[rsp + pointer], r!.Value);
            }
            else if (r is null && offset is null)
            {
                ViskThrowHelper.ThrowInvalidOperationException();
            }
            else
            {
                _dataManager.Assembler.mov(rax, offset!.Value);

                _dataManager.Assembler.mov(__[rsp + pointer], rax);
            }

        _stackChanged = totalSize;
    }

    public void SaveRegs()
    {
        if (_stackChanged != 0)
            ViskThrowHelper.ThrowInvalidOperationException("You must to move args after call this method");

        _regsCount = _dataManager.Register.CurIndex;
        for (var index = 0; index < _regsCount; index++)
        {
            var register = ViskRegister.PublicRegisters[index];
            _dataManager.Assembler.mov(_dataManager.FuncStackManager.GetMemoryToSaveReg(index), register);
        }

        _localsCount = Math.Min(_dataManager.CurrentFuncLocals.Locals.Count, ViskRegister.LocalsRegisters.Length);
        for (var index = 0; index < _localsCount; index++)
        {
            var register = ViskRegister.LocalsRegisters[index];
            _dataManager.Assembler.mov(
                _dataManager.FuncStackManager.GetMemoryToSaveReg(_regsCount + index),
                register
            );
        }
    }

    public void LoadRegs()
    {
        if (_stackChanged != 0)
            _dataManager.Assembler.add(rsp, _stackChanged);


        for (var index = _localsCount - 1; index >= 0; index--)
        {
            var register = ViskRegister.LocalsRegisters[index];
            _dataManager.Assembler.mov(
                register,
                _dataManager.FuncStackManager.GetMemoryToSaveReg(_regsCount + index)
            );
        }

        for (var index = _regsCount - 1; index >= 0; index--)
        {
            var register = ViskRegister.PublicRegisters[index];
            _dataManager.Assembler.mov(register, _dataManager.FuncStackManager.GetMemoryToSaveReg(index));
        }

        _stackChanged = 0;
    }

    public void SaveReturnValue(Type rt)
    {
        if (rt == typeof(void)) return;

        if (rt != typeof(long))
            ViskThrowHelper.ThrowInvalidOperationException("Unknown return type");

        if (_dataManager.Register.CanGetNext)
            _dataManager.Assembler.mov(_dataManager.Register.Next(), rax);
        else _dataManager.Assembler.mov(_dataManager.Stack.GetNext(), rax);
    }

    public void Reset() => _index = 0;

    public int NextArgIndex() => _index++;
}