namespace ViskCompiler;

using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal sealed class ViskCompiler
{
    private readonly ViskDataManager _dataManager;
    private readonly ArgsManager _argsManager;

    public ViskCompiler(ViskModule module)
    {
        _dataManager = new ViskDataManager(new Assembler(64), module);
        _argsManager = new ArgsManager(_dataManager);
    }

    public ViskX64AsmExecutor Compile()
    {
        CompileInternal();

        return new ViskX64AsmExecutor(_dataManager.Assembler);
    }

    private void CompileInternal()
    {
        CompileFunctions();

        DeclareData();
    }

    private void DeclareData()
    {
        foreach (var data in _dataManager.Data)
        {
            var dataItem1 = data.Item1;
            _dataManager.Assembler.Label(ref dataItem1);
            _dataManager.Assembler.dq(data.Item2);
        }
    }

    private void CompileFunctions()
    {
        var mainFunc = _dataManager.Module.Functions.First(x => x.Name == _dataManager.Module.MainFuncName);
        CompileFunction(mainFunc);

        foreach (var function in _dataManager.Module.Functions)
        {
            if (function == mainFunc)
                continue;

            CompileFunction(function);
        }
    }

    private void CompileFunction(ViskFunction func)
    {
        foreach (var instruction in func.TotalInstructions)
            CompileInstruction(instruction, func);
    }

    private void CompileInstruction(ViskInstruction instruction, ViskFunction func)
    {
        GetArgs(instruction, out var arg0, out var arg1, out var arg2);

        switch (instruction.InstructionKind)
        {
            case ViskInstructionKind.PushConst:
                Push(arg0.AsI64());
                break;
            case ViskInstructionKind.Add:
                Operate(_dataManager.Assembler.add, _dataManager.Assembler.add);
                break;
            case ViskInstructionKind.SetLabel:
                var label = _dataManager.GetLabel(arg0.As<string>());
                _dataManager.Assembler.Label(ref label);
                break;
            case ViskInstructionKind.Goto:
                _dataManager.Assembler.jmp(_dataManager.GetLabel(arg0.As<string>()));
                break;
            case ViskInstructionKind.GotoIfNotEquals:
                Push(0);
                Operate(_dataManager.Assembler.cmp, _dataManager.Assembler.cmp, false);
                _dataManager.Assembler.jne(_dataManager.GetLabel(arg0.As<string>()));
                break;
            case ViskInstructionKind.SetLocal:
                if (!_dataManager.Stack.IsEmpty())
                {
                    _dataManager.Assembler.mov(rax, _dataManager.Stack.GetPrevious());
                    _dataManager.Assembler.mov(_dataManager.CurrentFuncLocals[arg0.As<string>()], rax);
                }
                else
                {
                    _dataManager.Assembler.mov(
                        _dataManager.CurrentFuncLocals[arg0.As<string>()],
                        _dataManager.Register.Previous()
                    );
                }

                break;
            case ViskInstructionKind.LoadLocal:
                if (_dataManager.Register.CanGetNext)
                {
                    _dataManager.Assembler.mov(
                        _dataManager.Register.Next(),
                        _dataManager.CurrentFuncLocals[arg0.As<string>()]
                    );
                }
                else
                {
                    _dataManager.Assembler.mov(rax, _dataManager.CurrentFuncLocals[arg0.As<string>()]);
                    _dataManager.Assembler.mov(_dataManager.Stack.GetNext(), rax);
                }

                break;

            case ViskInstructionKind.Nop:
                _dataManager.Assembler.nop();
                break;
            case ViskInstructionKind.Cmp:
                Operate(_dataManager.Assembler.cmp, _dataManager.Assembler.cmp);

                if (_dataManager.Register.CanGetNext)
                {
                    _dataManager.Register.Previous();
                    var oldValue = _dataManager.Register.Current();
                    var assemblerRegister8 = _dataManager.Register.Next8();
                    _dataManager.Assembler.setne(assemblerRegister8);
                    _dataManager.Assembler.movzx(oldValue, assemblerRegister8);
                }
                else
                {
                    _dataManager.Assembler.setne(al);
                    _dataManager.Assembler.movzx(rax, al);
                    _dataManager.Assembler.mov(_dataManager.Stack.GetNext(), rax);
                }

                break;
            case ViskInstructionKind.Dup:
                if (_dataManager.Register.CanGetNext)
                {
                    var src = _dataManager.Register.BackValue();
                    var dst = _dataManager.Register.Next();

                    _dataManager.Assembler.mov(dst, src);
                }
                else
                {
                    var src = _dataManager.Stack.BackValue();
                    var dst = _dataManager.Stack.GetNext();

                    _dataManager.Assembler.mov(rax, src);
                    _dataManager.Assembler.mov(dst, rax);
                }

                break;
            case ViskInstructionKind.IMul:
                Operate(_dataManager.Assembler.imul, _dataManager.Assembler.imul);
                break;
            case ViskInstructionKind.Sub:
                Operate(_dataManager.Assembler.sub, _dataManager.Assembler.sub);
                break;
            case ViskInstructionKind.CallForeign:
                _argsManager.SaveRegs();
                _argsManager.ForeignMoveArgs(arg1.AsI32());

                _dataManager.Assembler.call((ulong)arg0.As<nint>());

                _argsManager.LoadRegs();
                _argsManager.SaveReturnValue(arg2.As<Type>());
                break;
            case ViskInstructionKind.Call:
                _argsManager.SaveRegs();
                _argsManager.MoveArgs(arg0.As<ViskFunction>().ArgsCount);

                _dataManager.Assembler.call(_dataManager.GetLabel(arg0.As<ViskFunction>().Name));

                _argsManager.LoadRegs();
                _argsManager.SaveReturnValue(arg0.As<ViskFunction>().ReturnType);
                break;
            case ViskInstructionKind.Prolog:
                label = _dataManager.GetLabel(func.Name);
                _dataManager.Assembler.Label(ref label);

                _dataManager.Assembler.push(rbp);
                _dataManager.Assembler.mov(rbp, rsp);

                _dataManager.NewFunc(func.MaxStackSize, func.Locals);
                _dataManager.Assembler.sub(rsp,
                    _dataManager.Stack.MaxStackSize +
                    ViskRegister.Registers.Length * ViskStack.BlockSize +
                    _dataManager.CurrentFuncLocalsSize
                );

                break;
            case ViskInstructionKind.Drop:
                Drop();
                break;
            case ViskInstructionKind.Ret:
                if (!_dataManager.Stack.IsEmpty())
                    _dataManager.Assembler.mov(rax, _dataManager.Stack.GetPrevious());
                else if (_dataManager.Register.CanGetPrevious)
                    _dataManager.Assembler.mov(rax, _dataManager.Register.Previous());

                _dataManager.Assembler.mov(rsp, rbp);
                _dataManager.Assembler.pop(rbp);
                _dataManager.Assembler.ret();

                break;
            case ViskInstructionKind.SetArg:
                _dataManager.Assembler.mov(rax, __[rbp + arg1.As<int>() * 8 + 16]);
                _dataManager.Assembler.mov(_dataManager.CurrentFuncLocals[arg0.As<string>()], rax);
                break;
            default:
                ThrowHelper.ThrowInvalidOperationException($"Unknown instruction: {instruction}");
                break;
        }
    }

    private void Drop()
    {
        if (!_dataManager.Stack.IsEmpty())
            _dataManager.Stack.GetPrevious();
        else _dataManager.Register.Previous();
    }

    private void Push(long number)
    {
        if (_dataManager.Register.CanGetNext)
        {
            _dataManager.Assembler.mov(
                _dataManager.Register.Next(),
                __[_dataManager.DefineI64(number)]
            );
        }
        else
        {
            _dataManager.Assembler.mov(rax, __[_dataManager.DefineI64(number)]);
            _dataManager.Assembler.mov(_dataManager.Stack.GetNext(), rax);
        }
    }

    private void Operate(Action<AssemblerRegister64, AssemblerMemoryOperand> mm,
        Action<AssemblerRegister64, AssemblerRegister64> rr, bool saveResult = true)
    {
        if (!_dataManager.Stack.IsEmpty())
        {
            var src = _dataManager.Stack.GetPrevious();
            if (!_dataManager.Stack.IsEmpty())
            {
                _dataManager.Assembler.mov(rax, _dataManager.Stack.GetPrevious());
                mm(rax, src);

                if (saveResult)
                    _dataManager.Assembler.mov(_dataManager.Stack.GetNext(), rax);
            }
            else
            {
                if (saveResult)
                    mm(_dataManager.Register.BackValue(), src);
                else mm(_dataManager.Register.Previous(), src);
            }
        }
        else
        {
            var srcReg = _dataManager.Register.Previous();
            if (saveResult)
                rr(_dataManager.Register.BackValue(), srcReg);
            else rr(_dataManager.Register.Previous(), srcReg);
        }
    }

    private static void GetArgs(ViskInstruction instruction, out object? o, out object? o1, out object? o2)
    {
        o = instruction.Arguments.Count > 0 ? instruction.Arguments[0] : null;
        o1 = instruction.Arguments.Count > 1 ? instruction.Arguments[1] : null;
        o2 = instruction.Arguments.Count > 2 ? instruction.Arguments[2] : null;
    }
}