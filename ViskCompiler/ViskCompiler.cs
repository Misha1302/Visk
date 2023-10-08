namespace ViskCompiler;

using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal sealed class ViskCompiler : ViskCompilerBase
{
    private const int FuncPrologSize = 16;

    public ViskCompiler(ViskModule module) : base(module)
    {
    }


    protected override void PushConst(InstructionArgs args)
    {
        Push(args[0].AsI64());
    }

    protected override void Add(InstructionArgs args)
    {
        Operate(DataManager.Assembler.add, DataManager.Assembler.add);
    }

    protected override void SetLabel(InstructionArgs args)
    {
        var label = DataManager.GetLabel(args[0].As<string>());
        DataManager.Assembler.Label(ref label);
    }

    protected override void Goto(InstructionArgs args)
    {
        DataManager.Assembler.jmp(DataManager.GetLabel(args[0].As<string>()));
    }

    protected override void GotoIfFalse(InstructionArgs args)
    {
        CmpValueAndZero();

        DataManager.Assembler.je(DataManager.GetLabel(args[0].As<string>()));
    }

    protected override void GotoIfTrue(InstructionArgs args)
    {
        CmpValueAndZero();

        DataManager.Assembler.jne(DataManager.GetLabel(args[0].As<string>()));
    }

    protected override void LogicNeg(InstructionArgs args)
    {
        PreviousStackOrReg(
            () =>
            {
                DataManager.Assembler.mov(rax, 0);
                DataManager.Assembler.cmp(DataManager.Stack.GetPrevious(), rax);
                DataManager.Assembler.sete(al);
                DataManager.Assembler.movzx(rax, al);
                DataManager.Assembler.mov(DataManager.Stack.GetNext(), rax);
            },
            () =>
            {
                DataManager.Assembler.cmp(DataManager.Register.Previous(), 0);
                DataManager.Assembler.sete(al);
                DataManager.Assembler.movzx(DataManager.Register.Next(), al);
            }
        );
    }

    protected override void SetLocal(InstructionArgs args)
    {
        PreviousStackOrReg(
            () =>
            {
                if (DataManager.CurrentFuncLocals.GetLocalPos(args[0].As<string>(), out var reg, out var mem))
                {
                    DataManager.Assembler.mov(reg!.Value, DataManager.Stack.GetPrevious());
                }
                else
                {
                    DataManager.Assembler.mov(rax, DataManager.Stack.GetPrevious());
                    DataManager.Assembler.mov(mem!.Value, rax);
                }
            },
            () =>
            {
                if (DataManager.CurrentFuncLocals.GetLocalPos(args[0].As<string>(), out var reg, out var mem))
                {
                    DataManager.Assembler.mov(reg!.Value, DataManager.Register.Previous());
                }
                else
                {
                    DataManager.Assembler.mov(rax, DataManager.Register.Previous());
                    DataManager.Assembler.mov(mem!.Value, rax);
                }
            }
        );
    }

    protected override void LoadLocal(InstructionArgs args)
    {
        if (DataManager.Register.CanGetNext)
        {
            if (DataManager.CurrentFuncLocals.GetLocalPos(args[0].As<string>(), out var reg, out var mem))
                DataManager.Assembler.mov(
                    DataManager.Register.Next(),
                    reg!.Value
                );
            else
                DataManager.Assembler.mov(
                    DataManager.Register.Next(),
                    mem!.Value
                );
        }
        else
        {
            DataManager.Assembler.mov(DataManager.Stack.GetNext(), rax);

            if (DataManager.CurrentFuncLocals.GetLocalPos(args[0].As<string>(), out var reg, out var mem))
            {
                DataManager.Assembler.mov(
                    DataManager.Stack.GetNext(),
                    reg!.Value
                );
            }
            else
            {
                DataManager.Assembler.mov(rax, mem!.Value);
                DataManager.Assembler.mov(
                    DataManager.Stack.GetNext(),
                    rax
                );
            }
        }
    }

    protected override void Nop(InstructionArgs args)
    {
        DataManager.Assembler.nop();
    }

    protected override void Cmp(InstructionArgs args)
    {
        Operate(DataManager.Assembler.cmp, DataManager.Assembler.cmp);

        if (!DataManager.Register.CanGetNext)
        {
            DataManager.Assembler.sete(al);
            DataManager.Assembler.movzx(rax, al);
            DataManager.Assembler.mov(DataManager.Stack.GetNext(), rax);
        }
        else
        {
            DataManager.Register.Previous();
            var oldValue = DataManager.Register.Current();
            var assemblerRegister8 = DataManager.Register.Next8();
            DataManager.Assembler.sete(assemblerRegister8);
            DataManager.Assembler.movzx(oldValue, assemblerRegister8);
        }
    }

    protected override void Dup(InstructionArgs args)
    {
        if (DataManager.Register.CanGetNext)
        {
            var src = DataManager.Register.BackValue();
            var dst = DataManager.Register.Next();

            DataManager.Assembler.mov(dst, src);
        }
        else
        {
            var src = DataManager.Stack.BackValue();
            var dst = DataManager.Stack.GetNext();

            DataManager.Assembler.mov(rax, src);
            DataManager.Assembler.mov(dst, rax);
        }
    }

    protected override void IMul(InstructionArgs args)
    {
        Operate(DataManager.Assembler.imul, DataManager.Assembler.imul);
    }

    protected override void Sub(InstructionArgs args)
    {
        Operate(DataManager.Assembler.sub, DataManager.Assembler.sub);
    }

    protected override void CallForeign(InstructionArgs args)
    {
        DataManager.ViskArgsManager.SaveRegs();
        DataManager.ViskArgsManager.ForeignMoveArgs(args[1].AsI32());

        DataManager.Assembler.call((ulong)args[0].As<nint>());

        DataManager.ViskArgsManager.LoadRegs();
        DataManager.ViskArgsManager.SaveReturnValue(args[2].As<Type>());
    }

    protected override void Call(InstructionArgs args)
    {
        DataManager.ViskArgsManager.SaveRegs();
        DataManager.ViskArgsManager.MoveArgs(args[0].As<ViskFunction>().ArgsCount);

        DataManager.Assembler.call(DataManager.GetLabel(args[0].As<ViskFunction>().Name));

        DataManager.ViskArgsManager.LoadRegs();
        DataManager.ViskArgsManager.SaveReturnValue(args[0].As<ViskFunction>().ReturnType);
    }

    protected override void Prolog(InstructionArgs args)
    {
        var label = DataManager.GetLabel(args.Function.Name);
        DataManager.Assembler.Label(ref label);

        DataManager.Assembler.push(rbp);
        DataManager.Assembler.mov(rbp, rsp);

        DataManager.NewFunc(args.Function.MaxStackSize, args.Function.Locals);


        var totalSize = DataManager.Stack.MaxStackSize +
                        ViskRegister.PublicRegisters.Length * ViskStack.BlockSize +
                        ViskRegister.LocalsRegisters.Length * ViskStack.BlockSize +
                        DataManager.CurrentFuncLocals.Size;
        DataManager.Assembler.sub(rsp, totalSize + totalSize % 16);
    }

    protected override void Drop(InstructionArgs args)
    {
        PreviousStackOrReg(
            () => DataManager.Stack.GetPrevious(),
            () => DataManager.Register.Previous()
        );
    }

    protected override void Ret(InstructionArgs args)
    {
        PreviousStackOrReg(
            () => DataManager.Assembler.mov(rax, DataManager.Stack.GetPrevious()),
            () => DataManager.Assembler.mov(rax, DataManager.Register.Previous()),
            () => { }
        );

        DataManager.Assembler.mov(rsp, rbp);
        DataManager.Assembler.pop(rbp);
        DataManager.Assembler.ret();
    }

    protected override void SetArg(InstructionArgs args)
    {
        if (DataManager.CurrentFuncLocals.GetLocalPos(args[0].As<string>(), out var reg, out var mem))
        {
            DataManager.Assembler.mov(reg!.Value,
                DataManager.FuncStackManager.GetMemoryArg(
                    DataManager.ViskArgsManager.NextArgIndex() * ViskStack.BlockSize + FuncPrologSize
                )
            );
        }
        else
        {
            DataManager.Assembler.mov(rax,
                DataManager.FuncStackManager.GetMemoryArg(
                    DataManager.ViskArgsManager.NextArgIndex() * ViskStack.BlockSize + FuncPrologSize
                )
            );
            DataManager.Assembler.mov(mem!.Value, rax);
        }
    }

    private void Push(long number)
    {
        NextStackOrReg(
            () =>
            {
                DataManager.Assembler.mov(rax, __[DataManager.DefineI64(number)]);
                DataManager.Assembler.mov(DataManager.Stack.GetNext(), rax);
            },
            () =>
            {
                DataManager.Assembler.mov(
                    DataManager.Register.Next(),
                    __[DataManager.DefineI64(number)]
                );
            }
        );
    }

    private void Operate(Action<AssemblerRegister64, AssemblerMemoryOperand> mm,
        Action<AssemblerRegister64, AssemblerRegister64> rr, bool saveResult = true)
    {
        PreviousStackOrReg(
            () =>
            {
                var src = DataManager.Stack.GetPrevious();
                PreviousStackOrReg(
                    () =>
                    {
                        DataManager.Assembler.mov(rax, DataManager.Stack.GetPrevious());
                        mm(rax, src);

                        if (saveResult)
                            DataManager.Assembler.mov(DataManager.Stack.GetNext(), rax);
                    },
                    () => mm(saveResult ? DataManager.Register.BackValue() : DataManager.Register.Previous(), src)
                );
            },
            () =>
            {
                var srcReg = DataManager.Register.Previous();
                rr(saveResult ? DataManager.Register.BackValue() : DataManager.Register.Previous(), srcReg);
            }
        );
    }

    private void PreviousStackOrReg(Action stack, Action reg, Action? onError = null)
    {
        if (!DataManager.Stack.IsEmpty()) stack();
        else if (DataManager.Register.CanGetPrevious) reg();
        else if (onError != null) onError();
        else ViskThrowHelper.ThrowInvalidOperationException("Stack and registers are already used");
    }

    private void NextStackOrReg(Action stack, Action reg)
    {
        if (DataManager.Register.CanGetNext) reg();
        else stack();
    }


    private void CmpValueAndZero()
    {
        PreviousStackOrReg(
            () =>
            {
                DataManager.Assembler.mov(rax, DataManager.Stack.GetPrevious());
                DataManager.Assembler.cmp(rax, 0);
            },
            () => { DataManager.Assembler.cmp(DataManager.Register.Previous(), 0); }
        );
    }
}