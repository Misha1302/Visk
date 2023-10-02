namespace ViskCompiler;

using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal sealed class ViskCompiler
{
    private readonly ViskDataManager _dataManager;

    public ViskCompiler(ViskModule module)
    {
        _dataManager = new ViskDataManager(new Assembler(64), module);
    }

    public ViskX64AsmExecutor Compile()
    {
        CompileFunctions();

        foreach (var data in _dataManager.Data)
        {
            var dataItem1 = data.Item1;
            _dataManager.Assembler.Label(ref dataItem1);
            _dataManager.Assembler.dq(data.Item2);
        }

        return new ViskX64AsmExecutor(_dataManager.Assembler);
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
                if (_dataManager.Register.CanGetNext)
                {
                    _dataManager.Assembler.mov(
                        _dataManager.Register.Next(),
                        __[_dataManager.DefineI64(arg0.AsI64())]
                    );
                }
                else
                {
                    _dataManager.Assembler.mov(rax, __[_dataManager.DefineI64(arg0.AsI64())]);
                    _dataManager.Assembler.mov(_dataManager.Stack.GetNext(), rax);
                }

                break;
            case ViskInstructionKind.Add:
                Action<AssemblerRegister64, AssemblerMemoryOperand> addMm = _dataManager.Assembler.add;
                Action<AssemblerRegister64, AssemblerRegister64> addRr = _dataManager.Assembler.add;

                Operate(addMm, addRr);

                break;
            case ViskInstructionKind.IMul:
                Action<AssemblerRegister64, AssemblerMemoryOperand> mulMm = _dataManager.Assembler.imul;
                Action<AssemblerRegister64, AssemblerRegister64> mulRr = _dataManager.Assembler.imul;

                Operate(mulMm, mulRr);

                break;
            case ViskInstructionKind.Sub:
                Action<AssemblerRegister64, AssemblerMemoryOperand> subMm = _dataManager.Assembler.sub;
                Action<AssemblerRegister64, AssemblerRegister64> subRr = _dataManager.Assembler.sub;

                Operate(subMm, subRr);

                break;
            case ViskInstructionKind.Prolog:
                _dataManager.Assembler.push(rbp);
                _dataManager.Assembler.mov(rbp, rsp);

                _dataManager.NewFunc(func.MaxStackSize);
                _dataManager.Assembler.sub(rsp,
                    _dataManager.Stack.MaxStackSize + ViskRegister.Registers.Length * ViskStack.BlockSize
                );
                _dataManager.Assembler.and(sp, ViskStack.StackAlignConst);

                break;
            case ViskInstructionKind.Ret:
                if (!_dataManager.Stack.IsEmpty())
                    _dataManager.Assembler.mov(rax, _dataManager.Stack.GetPrevious());
                else _dataManager.Assembler.mov(rax, _dataManager.Register.Previous());

                _dataManager.Assembler.mov(rsp, rbp);
                _dataManager.Assembler.pop(rbp);
                _dataManager.Assembler.ret();

                break;
        }
    }

    private void Operate(Action<AssemblerRegister64, AssemblerMemoryOperand> addMm,
        Action<AssemblerRegister64, AssemblerRegister64> addRr)
    {
        if (!_dataManager.Stack.IsEmpty())
        {
            var src = _dataManager.Stack.GetPrevious();
            if (!_dataManager.Stack.IsEmpty())
            {
                _dataManager.Assembler.mov(rax, _dataManager.Stack.GetPrevious());
                addMm(rax, src);
                _dataManager.Assembler.mov(_dataManager.Stack.GetNext(), rax);
            }
            else
            {
                addMm(_dataManager.Register.BackValue(), src);
            }
        }
        else
        {
            var srcReg = _dataManager.Register.Previous();
            addRr(_dataManager.Register.BackValue(), srcReg);
        }
    }

    private static void GetArgs(ViskInstruction instruction, out object? o, out object? o1, out object? o2)
    {
        o = instruction.Arguments.Count > 0 ? instruction.Arguments[0] : null;
        o1 = instruction.Arguments.Count > 1 ? instruction.Arguments[1] : null;
        o2 = instruction.Arguments.Count > 2 ? instruction.Arguments[2] : null;
    }
}