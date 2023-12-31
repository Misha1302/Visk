﻿namespace ViskCompiler;

internal sealed class ViskArgsManager
{
    private static readonly AssemblerRegister64[] _argsRegisters = { rcx, rdx, r8, r9 };
    private static readonly AssemblerRegisterXMM[] _argsRegistersD = { xmm0, xmm1, xmm2, xmm3 };

    private readonly ViskDataManager _dataManager;
    private int _stackChanged;
    private int _regsCount;
    private int _localsCount;
    private int _regsCountD;
    private int _localsCountD;
    private int _index;
    private int _x64Count;
    private int _xmmCount;

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
                if (_argsRegisters[j] != r.Value)
                    _dataManager.Assembler.mov(_argsRegisters[j], r.Value);
            }
            else if (xmm is not null)
            {
                if (_argsRegistersD[j] != xmm.Value)
                    _dataManager.Assembler.movq(_argsRegistersD[j], xmm.Value);
            }
            else
            {
                var jCopy = j;

                args[j].ChooseViskType(
                    () => _dataManager.Assembler.mov(_argsRegisters[jCopy], offset!.Value),
                    () => _dataManager.Assembler.movq(_argsRegistersD[jCopy], offset!.Value));
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

    public void SaveRegs(List<Type> argTypes)
    {
        if (_stackChanged != 0)
            ViskThrowHelper.ThrowInvalidOperationException("You must to move args after call this method");

        _x64Count = Math.Min(argTypes.Count(x => x == ViskConsts.I64), _argsRegisters.Length);
        _xmmCount = Math.Min(argTypes.Count(x => x == ViskConsts.F64), _argsRegistersD.Length);

        SaveX64();
        SaveD();
    }

    private void SaveX64()
    {
        _regsCount = _dataManager.Register.Rx64.CurIndex;
        for (var index = _x64Count; index < _regsCount; index++)
        {
            var register = ViskRegister.PublicRegisters[index];
            _dataManager.Assembler.mov(_dataManager.FuncStackManager.GetMemoryToSaveRegX64(index), register);
        }

        _localsCount = Math.Min(_dataManager.CurrentFuncLocals.Locals.Count, ViskLocal.LocalsRegisters.Length);
        for (var index = 0; index < _localsCount; index++)
        {
            var register = ViskLocal.LocalsRegisters[index];
            _dataManager.Assembler.mov(
                _dataManager.FuncStackManager.GetMemoryToSaveRegX64(_regsCount + index),
                register
            );
        }
    }

    private void SaveD()
    {
        _regsCountD = _dataManager.Register.Rx64.CurIndexD;
        for (var index = _xmmCount; index < _regsCountD; index++)
        {
            var register = ViskRegister.PublicRegistersD[index];
            _dataManager.Assembler.movq(_dataManager.FuncStackManager.GetMemoryToSaveRegD(index), register);
        }

        _localsCountD = Math.Min(_dataManager.CurrentFuncLocals.Locals.Count, ViskLocal.LocalsRegistersD.Length);
        for (var index = 0; index < _localsCountD; index++)
        {
            var register = ViskLocal.LocalsRegistersD[index];
            _dataManager.Assembler.movq(
                _dataManager.FuncStackManager.GetMemoryToSaveRegD(_regsCountD + index),
                register
            );
        }
    }

    public void LoadRegs()
    {
        if (_stackChanged != 0)
            _dataManager.Assembler.add(rsp, _stackChanged);


        LoadX64();
        LoadD();


        _stackChanged = _regsCountD = 0;
    }

    private void LoadX64()
    {
        for (var index = _regsCount - 1; index >= _x64Count; index--)
        {
            var register = ViskRegister.PublicRegisters[index];
            _dataManager.Assembler.mov(register, _dataManager.FuncStackManager.GetMemoryToSaveRegX64(index));
        }


        for (var index = _localsCount - 1; index >= 0; index--)
        {
            var register = ViskLocal.LocalsRegisters[index];
            _dataManager.Assembler.mov(
                register,
                _dataManager.FuncStackManager.GetMemoryToSaveRegX64(_regsCount + index)
            );
        }
    }

    private void LoadD()
    {
        for (var index = _regsCountD - 1; index >= _xmmCount; index--)
        {
            var register = ViskRegister.PublicRegistersD[index];
            _dataManager.Assembler.movq(register, _dataManager.FuncStackManager.GetMemoryToSaveRegD(index));
        }

        for (var index = _localsCountD - 1; index >= 0; index--)
        {
            var register = ViskLocal.LocalsRegistersD[index];
            _dataManager.Assembler.movq(
                register,
                _dataManager.FuncStackManager.GetMemoryToSaveRegD(_regsCountD + index)
            );
        }
    }

    public void SaveReturnValue(Type rt)
    {
        if (rt == ViskConsts.None)
            return;

        if (rt != ViskConsts.I64 && rt != ViskConsts.F64)
            ViskThrowHelper.ThrowInvalidOperationException("Unknown return type");

        if (rt == ViskConsts.I64)
        {
            if (_dataManager.Register.Rx64.CanGetNext)
                _dataManager.Assembler.mov(_dataManager.Register.Next(ViskConsts.I64).X64, rax);
            else _dataManager.Assembler.mov(_dataManager.Stack.GetNext(ViskConsts.I64), rax);
        }
        else
        {
            if (_dataManager.Register.Rd.CanGetNext)
                _dataManager.Assembler.movq(_dataManager.Register.Next(ViskConsts.F64).Xmm, xmm0);
            else _dataManager.Assembler.movq(_dataManager.Stack.GetNext(ViskConsts.F64), xmm0);
        }
    }

    public void Reset() => _index = 0;

    public int NextArgIndex() => _index++;
}