namespace ViskCompiler;

using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal sealed class ViskCompiler : ViskCompilerBase
{
    private const int FuncPrologSize = 16;

    public ViskCompiler(ViskModule module) : base(module)
    {
    }


    protected override void PushConst(object? arg0, object? arg1, object? arg2, ViskFunction func)
    {
        Push(arg0.AsI64());
    }

    protected override void Add(object? arg0, object? arg1, object? arg2, ViskFunction func)
    {
        Operate(DataManager.Assembler.add, DataManager.Assembler.add);
    }

    protected override void SetLabel(object? arg0, object? arg1, object? arg2, ViskFunction func)
    {
        var label = DataManager.GetLabel(arg0.As<string>());
        DataManager.Assembler.Label(ref label);
    }

    protected override void Goto(object? arg0, object? arg1, object? arg2, ViskFunction func)
    {
        DataManager.Assembler.jmp(DataManager.GetLabel(arg0.As<string>()));
    }

    protected override void GotoIfNotEquals(object? arg0, object? arg1, object? arg2, ViskFunction func)
    {
        Push(0);
        Operate(DataManager.Assembler.cmp, DataManager.Assembler.cmp, false);
        DataManager.Assembler.jne(DataManager.GetLabel(arg0.As<string>()));
    }

    protected override void SetLocal(object? arg0, object? arg1, object? arg2, ViskFunction func)
    {
        if (!DataManager.Stack.IsEmpty())
        {
            DataManager.Assembler.mov(rax, DataManager.Stack.GetPrevious());
            DataManager.Assembler.mov(DataManager.CurrentFuncLocals[arg0.As<string>()], rax);
        }
        else
        {
            DataManager.Assembler.mov(
                DataManager.CurrentFuncLocals[arg0.As<string>()],
                DataManager.Register.Previous()
            );
        }
    }

    protected override void LoadLocal(object? arg0, object? arg1, object? arg2, ViskFunction func)
    {
        if (DataManager.Register.CanGetNext)
        {
            DataManager.Assembler.mov(
                DataManager.Register.Next(),
                DataManager.CurrentFuncLocals[arg0.As<string>()]
            );
        }
        else
        {
            DataManager.Assembler.mov(rax, DataManager.CurrentFuncLocals[arg0.As<string>()]);
            DataManager.Assembler.mov(DataManager.Stack.GetNext(), rax);
        }
    }

    protected override void Nop(object? arg0, object? arg1, object? arg2, ViskFunction func)
    {
        DataManager.Assembler.nop();
    }

    protected override void Cmp(object? arg0, object? arg1, object? arg2, ViskFunction func)
    {
        Operate(DataManager.Assembler.cmp, DataManager.Assembler.cmp);

        if (DataManager.Register.CanGetNext)
        {
            DataManager.Register.Previous();
            var oldValue = DataManager.Register.Current();
            var assemblerRegister8 = DataManager.Register.Next8();
            DataManager.Assembler.setne(assemblerRegister8);
            DataManager.Assembler.movzx(oldValue, assemblerRegister8);
        }
        else
        {
            DataManager.Assembler.setne(al);
            DataManager.Assembler.movzx(rax, al);
            DataManager.Assembler.mov(DataManager.Stack.GetNext(), rax);
        }
    }

    protected override void Dup(object? arg0, object? arg1, object? arg2, ViskFunction func)
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

    protected override void IMul(object? arg0, object? arg1, object? arg2, ViskFunction func)
    {
        Operate(DataManager.Assembler.imul, DataManager.Assembler.imul);
    }

    protected override void Sub(object? arg0, object? arg1, object? arg2, ViskFunction func)
    {
        Operate(DataManager.Assembler.sub, DataManager.Assembler.sub);
    }

    protected override void CallForeign(object? arg0, object? arg1, object? arg2, ViskFunction func)
    {
        DataManager.ViskArgsManager.SaveRegs();
        DataManager.ViskArgsManager.ForeignMoveArgs(arg1.AsI32());

        DataManager.Assembler.call((ulong)arg0.As<nint>());

        DataManager.ViskArgsManager.LoadRegs();
        DataManager.ViskArgsManager.SaveReturnValue(arg2.As<Type>());
    }

    protected override void Call(object? arg0, object? arg1, object? arg2, ViskFunction func)
    {
        DataManager.ViskArgsManager.SaveRegs();
        DataManager.ViskArgsManager.MoveArgs(arg0.As<ViskFunction>().ArgsCount);

        DataManager.Assembler.call(DataManager.GetLabel(arg0.As<ViskFunction>().Name));

        DataManager.ViskArgsManager.LoadRegs();
        DataManager.ViskArgsManager.SaveReturnValue(arg0.As<ViskFunction>().ReturnType);
    }

    protected override void Prolog(object? arg0, object? arg1, object? arg2, ViskFunction func)
    {
        var label = DataManager.GetLabel(func.Name);
        DataManager.Assembler.Label(ref label);

        DataManager.Assembler.push(rbp);
        DataManager.Assembler.mov(rbp, rsp);

        DataManager.NewFunc(func.MaxStackSize, func.Locals);
        DataManager.Assembler.sub(rsp,
            DataManager.Stack.MaxStackSize +
            ViskRegister.Registers.Length * ViskStack.BlockSize +
            DataManager.CurrentFuncLocalsSize
        );
    }

    protected override void Drop(object? arg0, object? arg1, object? arg2, ViskFunction func)
    {
        if (!DataManager.Stack.IsEmpty())
            DataManager.Stack.GetPrevious();
        else DataManager.Register.Previous();
    }

    protected override void Ret(object? arg0, object? arg1, object? arg2, ViskFunction func)
    {
        if (!DataManager.Stack.IsEmpty())
            DataManager.Assembler.mov(rax, DataManager.Stack.GetPrevious());
        else if (DataManager.Register.CanGetPrevious)
            DataManager.Assembler.mov(rax, DataManager.Register.Previous());

        DataManager.Assembler.mov(rsp, rbp);
        DataManager.Assembler.pop(rbp);
        DataManager.Assembler.ret();
    }

    protected override void SetArg(object? arg0, object? arg1, object? arg2, ViskFunction func)
    {
        DataManager.Assembler.mov(rax,
            DataManager.FuncStackManager.GetMemoryArg(DataManager.ViskArgsManager.NextArgIndex() * ViskStack.BlockSize + FuncPrologSize));
        DataManager.Assembler.mov(DataManager.CurrentFuncLocals[arg0.As<string>()], rax);
    }

    private void Push(long number)
    {
        if (DataManager.Register.CanGetNext)
        {
            DataManager.Assembler.mov(
                DataManager.Register.Next(),
                __[DataManager.DefineI64(number)]
            );
        }
        else
        {
            DataManager.Assembler.mov(rax, __[DataManager.DefineI64(number)]);
            DataManager.Assembler.mov(DataManager.Stack.GetNext(), rax);
        }
    }

    private void Operate(Action<AssemblerRegister64, AssemblerMemoryOperand> mm,
        Action<AssemblerRegister64, AssemblerRegister64> rr, bool saveResult = true)
    {
        if (!DataManager.Stack.IsEmpty())
        {
            var src = DataManager.Stack.GetPrevious();
            if (!DataManager.Stack.IsEmpty())
            {
                DataManager.Assembler.mov(rax, DataManager.Stack.GetPrevious());
                mm(rax, src);

                if (saveResult)
                    DataManager.Assembler.mov(DataManager.Stack.GetNext(), rax);
            }
            else
            {
                mm(saveResult ? DataManager.Register.BackValue() : DataManager.Register.Previous(), src);
            }
        }
        else
        {
            var srcReg = DataManager.Register.Previous();
            rr(saveResult ? DataManager.Register.BackValue() : DataManager.Register.Previous(), srcReg);
        }
    }
}