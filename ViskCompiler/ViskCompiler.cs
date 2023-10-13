namespace ViskCompiler;

using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal sealed class ViskCompiler : ViskCompilerBase
{
    private const int FuncPrologSize = 8 + 8 + 8 + 8;

    public ViskCompiler(ViskModule module, ViskSettings settings) : base(module, settings)
    {
    }


    protected override void PushConst(InstructionArgs args)
    {
        Push(args[0].AsI64());
    }

    protected override void PushConstD(InstructionArgs args)
    {
        Push(args[0].AsF64());
    }

    protected override void Add(InstructionArgs args)
    {
        Operate(DataManager.Assembler.add, DataManager.Assembler.add);
    }

    protected override void AddD(InstructionArgs args)
    {
        OperateD(DataManager.Assembler.addsd, DataManager.Assembler.addsd);
    }

    protected override void SubD(InstructionArgs args)
    {
        OperateD(DataManager.Assembler.subsd, DataManager.Assembler.subsd);
    }

    protected override void MulD(InstructionArgs args)
    {
        OperateD(DataManager.Assembler.mulsd, DataManager.Assembler.mulsd);
    }

    protected override void DivD(InstructionArgs args)
    {
        OperateD(DataManager.Assembler.divsd, DataManager.Assembler.divsd);
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
        PreviousStackOrRegX64(
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
        PreviousStackOrRegX64(
            () =>
            {
                if (DataManager.CurrentFuncLocals.GetLocalPos(args[0].As<string>(), out var reg, out _, out var mem))
                {
                    DataManager.Assembler.mov(reg!.Value, DataManager.Register.Previous());
                }
                else
                {
                    DataManager.Assembler.mov(rax, DataManager.Register.Previous());
                    DataManager.Assembler.mov(mem!.Value, rax);
                }
            },
            () =>
            {
                if (DataManager.CurrentFuncLocals.GetLocalPos(args[0].As<string>(), out var reg, out _, out var mem))
                {
                    DataManager.Assembler.mov(reg!.Value, DataManager.Stack.GetPrevious());
                }
                else
                {
                    DataManager.Assembler.mov(rax, DataManager.Stack.GetPrevious());
                    DataManager.Assembler.mov(mem!.Value, rax);
                }
            }
        );
    }

    protected override void SetLocalD(InstructionArgs args)
    {
        PreviousStackOrRegD(() =>
        {
            if (DataManager.CurrentFuncLocals.GetLocalPos(args[0].As<string>(), out _, out var xmm, out var mem))
            {
                DataManager.Assembler.movq(xmm!.Value, DataManager.Register.PreviousD());
            }
            else
            {
                DataManager.Assembler.movq(rax, DataManager.Register.PreviousD());
                DataManager.Assembler.mov(mem!.Value, rax);
            }
        }, () =>
        {
            if (DataManager.CurrentFuncLocals.GetLocalPos(args[0].As<string>(), out _, out var xmm, out var mem))
            {
                DataManager.Assembler.movq(xmm!.Value, DataManager.Stack.GetPrevious());
            }
            else
            {
                DataManager.Assembler.mov(rax, DataManager.Stack.GetPrevious());
                DataManager.Assembler.mov(mem!.Value, rax);
            }
        });
    }

    protected override void LoadLocal(InstructionArgs args)
    {
        NextStackOrRegX64(() =>
            {
                if (DataManager.CurrentFuncLocals.GetLocalPos(args[0].As<string>(), out var reg, out _, out var mem))
                    DataManager.Assembler.mov(
                        DataManager.Register.Next(),
                        reg!.Value
                    );
                else
                    DataManager.Assembler.mov(
                        DataManager.Register.Next(),
                        mem!.Value
                    );
            },
            () =>
            {
                if (DataManager.CurrentFuncLocals.GetLocalPos(args[0].As<string>(), out var reg, out _, out var mem))
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
        );
    }

    protected override void LoadLocalD(InstructionArgs args)
    {
        NextStackOrRegXmm(() =>
            {
                if (DataManager.CurrentFuncLocals.GetLocalPos(args[0].As<string>(), out _, out var xmm, out var mem))
                    DataManager.Assembler.movq(
                        DataManager.Register.NextD(),
                        xmm!.Value
                    );
                else
                    DataManager.Assembler.movq(
                        DataManager.Register.NextD(),
                        mem!.Value
                    );
            }, () =>
            {
                if (DataManager.CurrentFuncLocals.GetLocalPos(args[0].As<string>(), out _, out var xmm, out var mem))
                {
                    DataManager.Assembler.movq(
                        DataManager.Stack.GetNext(),
                        xmm!.Value
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
        );
    }

    protected override void Nop(InstructionArgs args)
    {
        DataManager.Assembler.nop();
    }

    private void Cmp()
    {
        Operate(DataManager.Assembler.cmp, DataManager.Assembler.cmp);
    }

    protected override void Equals(InstructionArgs args)
    {
        Cmp();
        SaveFlagsAsBool(false);
    }

    protected override void NotEquals(InstructionArgs args)
    {
        Cmp();
        SaveFlagsAsBool(true);
    }


    private void SaveFlagsAsBool(bool invert)
    {
        if (!DataManager.Register.CanGetNext)
        {
            if (!invert)
                DataManager.Assembler.sete(al);
            else DataManager.Assembler.setne(al);

            DataManager.Assembler.movzx(rax, al);
            DataManager.Assembler.mov(DataManager.Stack.GetNext(), rax);
        }
        else
        {
            DataManager.Register.Previous();
            var oldValue = DataManager.Register.Current();
            var assemblerRegister8 = DataManager.Register.Next8();

            if (!invert)
                DataManager.Assembler.sete(assemblerRegister8);
            else DataManager.Assembler.setne(assemblerRegister8);

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

    protected override void IDiv(InstructionArgs args)
    {
        PreviousStackOrRegX64(
            () => DataManager.Assembler.mov(rax, DataManager.Register.Previous()),
            () => DataManager.Assembler.mov(rax, DataManager.Stack.GetPrevious())
        );

        PreviousStackOrRegX64(
            () =>
            {
                var r = DataManager.Register.Previous();
                DataManager.Assembler.xchg(r, rax);
                DataManager.Assembler.cqo();
                DataManager.Assembler.idiv(r);
            },
            () =>
            {
                var mem = DataManager.Stack.GetPrevious();
                DataManager.Assembler.mov(rdi, mem);
                DataManager.Assembler.xchg(rdi, rax);
                DataManager.Assembler.cqo();
                DataManager.Assembler.idiv(rdi);
            });

        NextStackOrRegX64(
            () => DataManager.Assembler.mov(DataManager.Register.Next(), rax),
            () => DataManager.Assembler.mov(DataManager.Stack.GetNext(), rax)
        );
    }

    protected override void CallForeign(InstructionArgs args)
    {
        DataManager.ViskArgsManager.SaveRegs();
        DataManager.ViskArgsManager.ForeignMoveArgs(args[1].As<List<Type>>());

        DataManager.Assembler.call((ulong)args[0].As<nint>());

        DataManager.ViskArgsManager.LoadRegs();
        DataManager.ViskArgsManager.SaveReturnValue(args[2].As<Type>());
    }

    protected override void Call(InstructionArgs args)
    {
        DataManager.ViskArgsManager.SaveRegs();
        DataManager.ViskArgsManager.MoveArgs(args[0].As<ViskFunction>().Info.Params);

        DataManager.Assembler.call(DataManager.GetLabel(args[0].As<ViskFunction>().Info.Name));

        DataManager.ViskArgsManager.LoadRegs();
        DataManager.ViskArgsManager.SaveReturnValue(args[0].As<ViskFunction>().Info.ReturnType);
    }

    protected override void Prolog(InstructionArgs args)
    {
        var label = DataManager.GetLabel(args.Function.Info.Name);
        DataManager.Assembler.Label(ref label);

        DataManager.Assembler.push(rbp);
        DataManager.Assembler.push(rdi);
        DataManager.Assembler.push(rsi);
        DataManager.Assembler.mov(rbp, rsp);

        DataManager.NewFunc(args.Function.MaxStackSize, args.Function.Locals);


        var totalSize = DataManager.Stack.MaxStackSize +
                        ViskRegister.PublicRegisters.Length * ViskStack.BlockSize +
                        ViskRegister.LocalsRegisters.Length * ViskStack.BlockSize +
                        ViskRegister.PublicRegistersD.Length * ViskStack.BlockSize +
                        ViskRegister.LocalsRegistersD.Length * ViskStack.BlockSize +
                        DataManager.CurrentFuncLocals.Size;
        DataManager.Assembler.sub(rsp, totalSize + totalSize % 16);
    }

    protected override void Drop(InstructionArgs args)
    {
        PreviousStackOrRegX64(
            () => DataManager.Stack.GetPrevious(),
            () => DataManager.Register.Previous()
        );
    }

    protected override void Ret(InstructionArgs args)
    {
        PreviousStackOrRegX64(
            () => DataManager.Assembler.mov(rax, DataManager.Register.Previous()),
            () => DataManager.Assembler.mov(rax, DataManager.Stack.GetPrevious()),
            () => { }
        );

        DataManager.Assembler.mov(rsp, rbp);
        DataManager.Assembler.pop(rsi);
        DataManager.Assembler.pop(rdi);
        DataManager.Assembler.pop(rbp);
        DataManager.Assembler.ret();
    }

    protected override void SetArg(InstructionArgs args)
    {
        if (DataManager.CurrentFuncLocals.GetLocalPos(args[0].As<string>(), out var reg, out _, out var mem))
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
        NextStackOrRegX64(
            () => { DataManager.Assembler.mov(DataManager.Register.Next(), __[DataManager.DefineI64(number)]); },
            () =>
            {
                DataManager.Assembler.mov(rax, __[DataManager.DefineI64(number)]);
                DataManager.Assembler.mov(DataManager.Stack.GetNext(), rax);
            }
        );
    }

    private void Push(double number)
    {
        NextStackOrRegXmm(
            () =>
            {
                DataManager.Assembler.movq(DataManager.Register.NextD(),
                    __[DataManager.DefineI64(BitConverter.DoubleToInt64Bits(number))]);
            },
            () =>
            {
                DataManager.Assembler.movq(xmm0, __[DataManager.DefineI64(BitConverter.DoubleToInt64Bits(number))]);
                DataManager.Assembler.movq(DataManager.Stack.GetNext(), xmm0);
            }
        );
    }

    private void Operate(Action<AssemblerRegister64, AssemblerMemoryOperand> mm,
        Action<AssemblerRegister64, AssemblerRegister64> rr, bool saveResult = true)
    {
        PreviousStackOrRegX64(
            () =>
            {
                var srcReg = DataManager.Register.Previous();
                rr(saveResult ? DataManager.Register.BackValue() : DataManager.Register.Previous(), srcReg);
            },
            () =>
            {
                var src = DataManager.Stack.GetPrevious();
                PreviousStackOrRegX64(
                    () => mm(saveResult ? DataManager.Register.BackValue() : DataManager.Register.Previous(), src),
                    () =>
                    {
                        DataManager.Assembler.mov(rax, DataManager.Stack.GetPrevious());
                        mm(rax, src);

                        if (saveResult)
                            DataManager.Assembler.mov(DataManager.Stack.GetNext(), rax);
                    }
                );
            }
        );
    }

    private void OperateD(Action<AssemblerRegisterXMM, AssemblerMemoryOperand> mm,
        Action<AssemblerRegisterXMM, AssemblerRegisterXMM> rr, bool saveResult = true)
    {
        PreviousStackOrRegD(() =>
        {
            var srcReg = DataManager.Register.PreviousD();
            rr(saveResult ? DataManager.Register.BackValueD() : DataManager.Register.PreviousD(), srcReg);
        }, () =>
        {
            var src = DataManager.Stack.GetPrevious();
            PreviousStackOrRegD(
                () => mm(saveResult ? DataManager.Register.BackValueD() : DataManager.Register.PreviousD(), src), () =>
                {
                    DataManager.Assembler.movq(xmm0, DataManager.Stack.GetPrevious());
                    mm(xmm0, src);

                    if (saveResult)
                        DataManager.Assembler.movq(DataManager.Stack.GetNext(), xmm0);
                });
        });
    }

    private void PreviousStackOrRegX64(Action reg, Action stack, Action? onError = null)
    {
        if (!DataManager.Stack.IsEmpty()) stack();
        else if (DataManager.Register.CanGetPrevious) reg();
        else if (onError != null) onError();
        else ViskThrowHelper.ThrowInvalidOperationException("Stack and registers are already used");
    }

    private void PreviousStackOrRegD(Action reg, Action stack, Action? onError = null)
    {
        if (!DataManager.Stack.IsEmpty()) stack();
        else if (DataManager.Register.CanGetPreviousD) reg();
        else if (onError != null) onError();
        else ViskThrowHelper.ThrowInvalidOperationException("Stack and registers are already used");
    }

    private void NextStackOrRegX64(Action reg, Action stack)
    {
        if (DataManager.Register.CanGetNext) reg();
        else stack();
    }

    private void NextStackOrRegXmm(Action xmm, Action stack)
    {
        if (DataManager.Register.CanGetNextD) xmm();
        else stack();
    }


    private void CmpValueAndZero()
    {
        PreviousStackOrRegX64(
            () => { DataManager.Assembler.cmp(DataManager.Register.Previous(), 0); },
            () =>
            {
                DataManager.Assembler.mov(rax, DataManager.Stack.GetPrevious());
                DataManager.Assembler.cmp(rax, 0);
            }
        );
    }
}