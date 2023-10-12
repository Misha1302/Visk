namespace ViskCompiler;

using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal sealed class ViskArgsManager
{
    private static readonly AssemblerRegister64[] _argsRegisters = { rcx, rdx, r8, r9 };
    private static readonly AssemblerRegisterXMM[] _argsRegistersD = { xmm0, xmm1, xmm2, xmm3 };

    private readonly ViskDataManager _dataManager;
    private int _stackChanged;
    private int _regsCount;
    private int _localsCount;
    private int _index;

    public ViskArgsManager(ViskDataManager dataManager)
    {
        _dataManager = dataManager;
    }

    public void ForeignMoveArgs(List<Type> args)
    {
        var regOfOffset = new ViskRegOrOffset(_dataManager.Stack, _dataManager.Register);

        if (args.Count >= _argsRegisters.Length)
        {
            var size = (args.Count - _argsRegisters.Length) * 8;
            var maxSize = 32 + size;

            var delta = maxSize + (ViskStack.PosStackAlign - maxSize % ViskStack.PosStackAlign);
            _stackChanged += delta;
            if (delta != 0)
                _dataManager.Assembler.sub(rsp, delta);

            var i = maxSize - 8;
            for (var j = 0; j < args.Count - _argsRegisters.Length; j++, i -= 8)
            {
                regOfOffset.GetRegisterOrOffset(args[j], out var r, out var xmm, out var offset);

                if (r is not null)
                {
                    _dataManager.Assembler.mov(__[rsp + i], r.Value);
                }
                else if (xmm is not null)
                {
                    _dataManager.Assembler.movq(__[rsp + i], xmm.Value);
                }
                else
                {
                    _dataManager.Assembler.mov(rax, offset!.Value);
                    _dataManager.Assembler.mov(__[rsp + i], rax);
                }
            }
        }

        var min = Math.Min(args.Count, _argsRegisters.Length);
        for (var j = min - 1; j >= 0; j--)
        {
            regOfOffset.GetRegisterOrOffset(args[j], out var r, out var xmm, out var offset);

            if (r is not null)
            {
                _dataManager.Assembler.mov(_argsRegisters[j], r.Value);
            }
            else if (xmm is not null)
            {
                _dataManager.Assembler.movq(_argsRegistersD[j], xmm.Value);
            }
            else
            {
                _dataManager.Assembler.mov(rax, offset!.Value);
                _dataManager.Assembler.mov(_argsRegisters[j], rax);
            }
        }
    }

    // it's stdcall i guess?
    public void MoveArgs(List<Type> args)
    {
        var regOfOffset = new ViskRegOrOffset(_dataManager.Stack, _dataManager.Register);

        var totalSize = args.Count * 8 + args.Count * 8 % 16;
        _dataManager.Assembler.sub(rsp, totalSize);
        var pointer = args.Count * 8 - 8;

        for (var i = 0; i < args.Count; i++, pointer -= 8)
        {
            regOfOffset.GetRegisterOrOffset(args[i], out var r, out var xmm, out var offset);

            if (r is not null)
            {
                _dataManager.Assembler.mov(__[rsp + pointer], r.Value);
            }
            else if (xmm is not null)
            {
                _dataManager.Assembler.movq(__[rsp + pointer], xmm.Value);
            }
            else
            {
                _dataManager.Assembler.mov(rax, offset!.Value);
                _dataManager.Assembler.mov(__[rsp + pointer], rax);
            }
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