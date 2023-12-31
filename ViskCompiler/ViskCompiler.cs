namespace ViskCompiler;

internal sealed class ViskCompiler : ViskCompilerBase
{
    private const int FuncPrologSize = 8 + 8 + 8 + 8;

    private readonly ViskBuildInFunctions _functions = new();

    public ViskCompiler(ViskModule module, ViskSettings settings) : base(module, settings)
    {
    }


    protected override void PushConst(InstructionArgs args)
    {
        Push(args[0].AsI64());
    }

    protected override void CallBuildIn(InstructionArgs args)
    {
        var argTypes = new List<Type> { typeof(long) };

        DataManager.ViskArgsManager.SaveRegs(argTypes);
        DataManager.ViskArgsManager.ForeignMoveArgs(argTypes);

        var buildIn = args[0].As<ViskBuildIn>();
        DataManager.Assembler.call(
            buildIn switch
            {
                ViskBuildIn.Alloc => (ulong)_functions.AllocPtr,
                ViskBuildIn.Free => (ulong)_functions.FreePtr,
                _ => ViskThrowHelper.ThrowInvalidOperationException<ulong>("Unknown buildIn call")
            }
        );

        DataManager.ViskArgsManager.LoadRegs();
        DataManager.ViskArgsManager.SaveReturnValue(typeof(long));
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
        OperateD(DataManager.Assembler.addsd);
    }

    protected override void SubD(InstructionArgs args)
    {
        OperateD(DataManager.Assembler.subsd);
    }

    protected override void MulD(InstructionArgs args)
    {
        OperateD(DataManager.Assembler.mulsd);
    }

    protected override void DivD(InstructionArgs args)
    {
        OperateD(DataManager.Assembler.divsd);
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

    protected override void IfFalse(InstructionArgs args)
    {
        ConditionBranch(args, DataManager.Assembler.je);
    }

    protected override void IfTrue(InstructionArgs args)
    {
        ConditionBranch(args, DataManager.Assembler.jne);
    }

    private void ConditionBranch(InstructionArgs args, Action<Label> condition)
    {
        DeconstructIfBlocks(args, out var ifBlock, out var elseBlock);

        CmpValueWithZero();

        var ifLabel = DataManager.GetLabel($"ifLabel_{Guid.NewGuid().ToString()}");
        var endLabel = DataManager.GetLabel($"endLabel_{Guid.NewGuid().ToString()}");

        DataManager.SaveState();

        condition(ifLabel);
        elseBlock.ForEach(x => CompileInstruction(x, args.Function));
        DataManager.Assembler.jmp(endLabel);

        DataManager.LoadState();
        DataManager.Assembler.Label(ref ifLabel);
        ifBlock.ForEach(x => CompileInstruction(x, args.Function));

        DataManager.Assembler.Label(ref endLabel);
    }

    private static void DeconstructIfBlocks(InstructionArgs args, out List<ViskInstruction> ifBlock,
        out List<ViskInstruction> elseBlock)
    {
        ifBlock = (List<ViskInstruction>)args[0];
        elseBlock = (List<ViskInstruction>)args[1];
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
                DataManager.Assembler.mov(DataManager.Stack.GetNext(ViskConsts.I64), rax);
            },
            () =>
            {
                DataManager.Assembler.cmp(DataManager.Register.Previous().X64, 0);
                DataManager.Assembler.sete(al);
                DataManager.Assembler.movzx(DataManager.Register.Next(ViskConsts.I64).X64, al);
            }
        );
    }

    protected override void SetLocal(InstructionArgs args)
    {
        PreviousStackOrRegX64(
            () =>
            {
                if (DataManager.CurrentFuncLocals.GetLocalPos(args[0].As<string>(), out var reg, out _, out var mem))
                    DataManager.Assembler.mov(reg!.Value, DataManager.Register.Previous().X64);
                else DataManager.Assembler.mov(mem!.Value, DataManager.Register.Previous().X64);
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
                DataManager.Assembler.movq(xmm!.Value, DataManager.Register.Previous().Xmm);
            }
            else
            {
                DataManager.Assembler.movq(rax, DataManager.Register.Previous().Xmm);
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

    protected override void LoadRef(InstructionArgs args)
    {
        var exceptionMessage = $"Cannot get (load) address (reference) of register ({args[0].As<string>()})";

        NextStackOrRegX64(() =>
            {
                DataManager.Assembler.lea
                (
                    DataManager.Register.Next(ViskConsts.I64).X64,
                    DataManager.CurrentFuncLocals.GetLocalPos(args[0].As<string>(), out _, out _, out var mem)
                        ? ViskThrowHelper.ThrowInvalidOperationException<AssemblerMemoryOperand>(exceptionMessage)
                        : mem!.Value
                );
            },
            () =>
            {
                DataManager.Assembler.lea
                (
                    rax,
                    DataManager.CurrentFuncLocals.GetLocalPos(args[0].As<string>(), out _, out _, out var mem)
                        ? ViskThrowHelper.ThrowInvalidOperationException<AssemblerMemoryOperand>(exceptionMessage)
                        : mem!.Value
                );

                DataManager.Assembler.mov(DataManager.Stack.GetNext(ViskConsts.I64), rax);
            }
        );
    }

    protected override void SetByRef(InstructionArgs args)
    {
        var argLen = args[0].As<int>();

        PreviousStackOrRegX64(
            () =>
            {
                var pos = DataManager.Register.Previous().X64;

                switch (argLen)
                {
                    case sizeof(long):
                        PreviousStackOrRegX64(
                            () => { DataManager.Assembler.mov(__[pos], DataManager.Register.Previous().X64); },
                            () =>
                            {
                                DataManager.Assembler.mov(rax, DataManager.Stack.GetPrevious());
                                DataManager.Assembler.mov(__[pos], rax);
                            }
                        );
                        break;
                    case sizeof(char):
                        PreviousStackOrRegX64(
                            () => DataManager.Assembler.mov(__dword_ptr[pos], DataManager.Register.Previous().X16),
                            () =>
                            {
                                DataManager.Assembler.mov(rax, DataManager.Stack.GetPrevious());
                                DataManager.Assembler.mov(__[pos], ax);
                            }
                        );
                        break;
                    default:
                        ViskThrowHelper.ThrowInvalidOperationException($"Invalid len ({argLen})");
                        break;
                }
            },
            () =>
            {
                var pos = DataManager.Stack.GetPrevious();

                switch (argLen)
                {
                    case sizeof(long):
                        PreviousStackOrRegX64(
                            () => DataManager.Assembler.mov(__[pos], DataManager.Register.Previous().X64),
                            () =>
                            {
                                DataManager.Assembler.mov(rax, DataManager.Stack.GetPrevious());
                                DataManager.Assembler.mov(rdi, pos);
                                DataManager.Assembler.mov(__[rdi], rax);
                            }
                        );
                        break;
                    case sizeof(char):
                        PreviousStackOrRegX64(
                            () => DataManager.Assembler.mov(__[pos], DataManager.Register.Previous().X16),
                            () =>
                            {
                                DataManager.Assembler.mov(rax, DataManager.Stack.GetPrevious());
                                DataManager.Assembler.mov(rdi, pos);
                                DataManager.Assembler.mov(__[rdi], ax);
                            }
                        );
                        break;
                    default:
                        ViskThrowHelper.ThrowInvalidOperationException($"Invalid len ({argLen})");
                        break;
                }
            }
        );
    }

    protected override void LoadByRef(InstructionArgs args)
    {
        NextStackOrRegX64(
            () =>
            {
                var pos = DataManager.Register.Previous().X64;

                DataManager.Assembler.mov(DataManager.Register.Next(ViskConsts.I64).X64, __[pos]);
            },
            () =>
            {
                var pos = DataManager.Stack.GetPrevious();

                DataManager.Assembler.mov(rax, __[pos]);
                DataManager.Assembler.mov(DataManager.Stack.GetNext(ViskConsts.I64), rax);
            }
        );
    }

    protected override void LoadByRefD(InstructionArgs args)
    {
        NextStackOrRegX64(
            () =>
            {
                var pos = DataManager.Register.Previous().X64;

                DataManager.Assembler.movq(DataManager.Register.Next(ViskConsts.F64).Xmm, __[pos]);
            },
            () =>
            {
                var pos = DataManager.Stack.GetPrevious();

                DataManager.Assembler.mov(rax, __[pos]);
                DataManager.Assembler.mov(DataManager.Stack.GetNext(ViskConsts.F64), rax);
            }
        );
    }

    protected override void SetByRefD(InstructionArgs args)
    {
        PreviousStackOrRegD(
            () =>
            {
                var pos = DataManager.Register.Previous().X64;

                PreviousStackOrRegD(
                    () => { DataManager.Assembler.movq(__[pos], DataManager.Register.Previous().Xmm); },
                    () =>
                    {
                        DataManager.Assembler.mov(rax, DataManager.Stack.GetPrevious());
                        DataManager.Assembler.mov(__[pos], rax);
                    }
                );
            },
            () =>
            {
                var pos = DataManager.Stack.GetPrevious();

                PreviousStackOrRegD(
                    () => { DataManager.Assembler.movq(__[pos], DataManager.Register.Previous().Xmm); },
                    () =>
                    {
                        DataManager.Assembler.mov(rax, DataManager.Stack.GetPrevious());
                        DataManager.Assembler.mov(__[pos], rax);
                    }
                );
            }
        );
    }

    protected override void LoadLocal(InstructionArgs args)
    {
        NextStackOrRegX64(() =>
            {
                if (DataManager.CurrentFuncLocals.GetLocalPos(args[0].As<string>(), out var reg, out _, out var mem))
                    DataManager.Assembler.mov(
                        DataManager.Register.Next(ViskConsts.I64).X64,
                        reg!.Value
                    );
                else
                    DataManager.Assembler.mov(
                        DataManager.Register.Next(ViskConsts.I64).X64,
                        mem!.Value
                    );
            },
            () =>
            {
                if (DataManager.CurrentFuncLocals.GetLocalPos(args[0].As<string>(), out var reg, out _, out var mem))
                {
                    DataManager.Assembler.mov(
                        DataManager.Stack.GetNext(ViskConsts.I64),
                        reg!.Value
                    );
                }
                else
                {
                    DataManager.Assembler.mov(rax, mem!.Value);
                    DataManager.Assembler.mov(
                        DataManager.Stack.GetNext(ViskConsts.I64),
                        rax
                    );
                }
            }
        );
    }

    protected override void LoadLocalD(InstructionArgs args)
    {
        NextStackOrRegD(() =>
            {
                if (DataManager.CurrentFuncLocals.GetLocalPos(args[0].As<string>(), out _, out var xmm, out var mem))
                    DataManager.Assembler.movq(
                        DataManager.Register.Next(ViskConsts.F64).Xmm,
                        xmm!.Value
                    );
                else
                    DataManager.Assembler.movq(
                        DataManager.Register.Next(ViskConsts.F64).Xmm,
                        mem!.Value
                    );
            }, () =>
            {
                if (DataManager.CurrentFuncLocals.GetLocalPos(args[0].As<string>(), out _, out var xmm, out var mem))
                {
                    DataManager.Assembler.movq(
                        DataManager.Stack.GetNext(ViskConsts.F64),
                        xmm!.Value
                    );
                }
                else
                {
                    DataManager.Assembler.mov(rax, mem!.Value);
                    DataManager.Assembler.mov(
                        DataManager.Stack.GetNext(ViskConsts.F64),
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
        CmpAsBool(Cmp, DataManager.Assembler.sete);
    }

    protected override void LessThan(InstructionArgs args)
    {
        CmpAsBool(Cmp, DataManager.Assembler.setl);
    }

    protected override void GreaterThan(InstructionArgs args)
    {
        CmpAsBool(Cmp, DataManager.Assembler.setg);
    }

    protected override void LessThanOrEquals(InstructionArgs args)
    {
        CmpAsBool(Cmp, DataManager.Assembler.setle);
    }

    protected override void GreaterThanOrEquals(InstructionArgs args)
    {
        CmpAsBool(Cmp, DataManager.Assembler.setge);
    }

    protected override void NotEquals(InstructionArgs args)
    {
        CmpAsBool(Cmp, DataManager.Assembler.setne);
    }

    protected override void EqualsD(InstructionArgs args)
    {
        CmpD(DoubleOperation.AreEqual);
    }

    protected override void LessThanD(InstructionArgs args)
    {
        CmpD(DoubleOperation.LessThan);
    }

    protected override void GreaterThanD(InstructionArgs args)
    {
        CmpD(DoubleOperation.GreaterThan);
    }

    protected override void LessThanOrEqualsD(InstructionArgs args)
    {
        CmpD(DoubleOperation.LessThanOrEquals);
    }

    protected override void GreaterThanOrEqualsD(InstructionArgs args)
    {
        CmpD(DoubleOperation.GreaterThanOrEquals);
    }

    protected override void NotEqualsD(InstructionArgs args)
    {
        CmpD(DoubleOperation.AreNotEquals);
    }


    private void CmpAsBool(Action cmp, Action<AssemblerRegister8> act)
    {
        cmp();

        if (!DataManager.Register.Rx64.CanGetNext)
        {
            act(al);

            DataManager.Assembler.movzx(rax, al);
            DataManager.Assembler.mov(DataManager.Stack.GetNext(ViskConsts.I64), rax);
        }
        else
        {
            DataManager.Register.Previous();
            var oldValue = DataManager.Register.Rx64.Current();
            var assemblerRegister8 = ViskRegister.PublicRegisters8[DataManager.Register.Next(ViskConsts.I64).X64];

            act(assemblerRegister8);

            DataManager.Assembler.movzx(oldValue, assemblerRegister8);
        }
    }

    protected override void Dup(InstructionArgs args)
    {
        if (DataManager.Register.Rx64.CanGetNext)
        {
            var src = DataManager.Register.BackValue().X64;
            var dst = DataManager.Register.Next(ViskConsts.I64).X64;

            DataManager.Assembler.mov(dst, src);
        }
        else
        {
            var src = DataManager.Stack.BackValue();
            var dst = DataManager.Stack.GetNext(ViskConsts.I64);

            DataManager.Assembler.mov(rax, src);
            DataManager.Assembler.mov(dst, rax);
        }
    }

    protected override void DupD(InstructionArgs args)
    {
        if (DataManager.Register.Rd.CanGetNext)
        {
            var src = DataManager.Register.BackValue().Xmm;
            var dst = DataManager.Register.Next(ViskConsts.F64).Xmm;

            DataManager.Assembler.movq(dst, src);
        }
        else
        {
            var src = DataManager.Stack.BackValue();
            var dst = DataManager.Stack.GetNext(ViskConsts.F64);

            DataManager.Assembler.movq(xmm0, src);
            DataManager.Assembler.movq(dst, xmm0);
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
            () => DataManager.Assembler.mov(rax, DataManager.Register.Previous().X64),
            () => DataManager.Assembler.mov(rax, DataManager.Stack.GetPrevious())
        );

        PreviousStackOrRegX64(
            () =>
            {
                var r = DataManager.Register.Previous().X64;
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
            () => DataManager.Assembler.mov(DataManager.Register.Next(ViskConsts.I64).X64, rax),
            () => DataManager.Assembler.mov(DataManager.Stack.GetNext(ViskConsts.I64), rax)
        );
    }

    protected override void CallForeign(InstructionArgs args)
    {
        var argTypes = args[1].As<List<Type>>();

        DataManager.ViskArgsManager.SaveRegs(argTypes);
        DataManager.ViskArgsManager.ForeignMoveArgs(argTypes);

        DataManager.Assembler.call((ulong)args[0].As<nint>());

        DataManager.ViskArgsManager.LoadRegs();
        DataManager.ViskArgsManager.SaveReturnValue(args[2].As<Type>());
    }

    protected override void Call(InstructionArgs args)
    {
        var argTypes = args[0].As<ViskFunction>().Info.Params;

        DataManager.ViskArgsManager.SaveRegs(argTypes);
        DataManager.ViskArgsManager.MoveArgs(argTypes);

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
        if (args.Function.Info.IsMain)
            SaveAllRegisters();
        DataManager.Assembler.mov(rbp, rsp);

        DataManager.NewFunc(args.Function.MaxStackSize, args.Function.Locals);


        var totalSize = DataManager.Stack.MaxStackSize +
                        ViskRegister.PublicRegisters.Length * ViskStack.BlockSize +
                        ViskLocal.LocalsRegisters.Length * ViskStack.BlockSize +
                        ViskRegister.PublicRegistersD.Length * ViskStack.BlockSize +
                        ViskLocal.LocalsRegistersD.Length * ViskStack.BlockSize +
                        DataManager.CurrentFuncLocals.Size;
        DataManager.Assembler.sub(rsp, totalSize + totalSize % 16);
    }

    protected override void Drop(InstructionArgs args)
    {
        PreviousStackOrRegX64(
            () => DataManager.Register.Previous(),
            () => DataManager.Stack.GetPrevious()
        );
    }

    protected override void DropD(InstructionArgs args)
    {
        PreviousStackOrRegD(
            () => DataManager.Register.Previous(),
            () => DataManager.Stack.GetPrevious()
        );
    }

    protected override void Ret(InstructionArgs args)
    {
        PreviousStackOrRegX64(
            () => DataManager.Assembler.mov(rax, DataManager.Register.Previous().X64),
            () => DataManager.Assembler.mov(rax, DataManager.Stack.GetPrevious()),
            () => { }
        );

        RetInternal(args);
    }

    protected override void RetD(InstructionArgs args)
    {
        PreviousStackOrRegD(
            () => DataManager.Assembler.movq(xmm0, DataManager.Register.Previous().Xmm),
            () => DataManager.Assembler.movq(xmm0, DataManager.Stack.GetPrevious()),
            () => { }
        );

        RetInternal(args);
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

    protected override void SetArgD(InstructionArgs args)
    {
        if (DataManager.CurrentFuncLocals.GetLocalPos(args[0].As<string>(), out _, out var xmm, out var mem))
        {
            DataManager.Assembler.movq(xmm!.Value,
                DataManager.FuncStackManager.GetMemoryArg(
                    DataManager.ViskArgsManager.NextArgIndex() * ViskStack.BlockSize + FuncPrologSize
                )
            );
        }
        else
        {
            DataManager.Assembler.movq(xmm0,
                DataManager.FuncStackManager.GetMemoryArg(
                    DataManager.ViskArgsManager.NextArgIndex() * ViskStack.BlockSize + FuncPrologSize
                )
            );
            DataManager.Assembler.movq(mem!.Value, xmm0);
        }
    }

    private void Push(long number)
    {
        var mem = __[DataManager.DefineI64(number)];

        NextStackOrRegX64(
            () => DataManager.Assembler.mov(DataManager.Register.Next(ViskConsts.I64).X64, mem),
            () =>
            {
                DataManager.Assembler.mov(rax, mem);
                DataManager.Assembler.mov(DataManager.Stack.GetNext(ViskConsts.I64), rax);
            }
        );
    }

    private void RetInternal(InstructionArgs args)
    {
        DataManager.Assembler.mov(rsp, rbp);
        if (args.Function.Info.IsMain)
            LoadAllRegisters();
        DataManager.Assembler.pop(rsi);
        DataManager.Assembler.pop(rdi);
        DataManager.Assembler.pop(rbp);
        DataManager.Assembler.ret();
    }

    private void Push(double number)
    {
        var mem = __[DataManager.DefineI64(BitConverter.DoubleToInt64Bits(number))];
        NextStackOrRegD(
            () => { DataManager.Assembler.movq(DataManager.Register.Next(ViskConsts.F64).Xmm, mem); },
            () =>
            {
                DataManager.Assembler.movq(xmm0, mem);
                DataManager.Assembler.movq(DataManager.Stack.GetNext(ViskConsts.F64), xmm0);
            }
        );
    }

    private void Operate(Action<AssemblerRegister64, AssemblerMemoryOperand> mm,
        Action<AssemblerRegister64, AssemblerRegister64> rr, bool saveResult = true)
    {
        PreviousStackOrRegX64(
            () =>
            {
                var srcReg = DataManager.Register.Previous().X64;
                rr(saveResult ? DataManager.Register.BackValue().X64 : DataManager.Register.Previous().X64, srcReg);
            },
            () =>
            {
                var src = DataManager.Stack.GetPrevious();
                PreviousStackOrRegX64(
                    () => mm(saveResult ? DataManager.Register.BackValue().X64 : DataManager.Register.Previous().X64,
                        src),
                    () =>
                    {
                        DataManager.Assembler.mov(rax, DataManager.Stack.GetPrevious());
                        mm(rax, src);

                        if (saveResult)
                            DataManager.Assembler.mov(DataManager.Stack.GetNext(ViskConsts.I64), rax);
                    }
                );
            }
        );
    }

    private void OperateD(Action<AssemblerRegisterXMM, AssemblerRegisterXMM> rr, bool saveResult = true)
    {
        PreviousStackOrRegD(() =>
        {
            var srcReg = DataManager.Register.Previous().Xmm;
            rr(saveResult ? DataManager.Register.BackValue().Xmm : DataManager.Register.Previous().Xmm, srcReg);
        }, () =>
        {
            var src = xmm1;
            DataManager.Assembler.movq(src, DataManager.Stack.GetPrevious());

            PreviousStackOrRegD(
                () => rr(saveResult ? DataManager.Register.BackValue().Xmm : DataManager.Register.Previous().Xmm, src),
                () =>
                {
                    DataManager.Assembler.movq(xmm0, DataManager.Stack.GetPrevious());
                    rr(xmm0, src);

                    if (saveResult)
                        DataManager.Assembler.movq(DataManager.Stack.GetNext(ViskConsts.I64), xmm0);
                });
        });
    }

    private void PreviousStackOrRegX64(Action reg, Action stack, Action? onError = null)
    {
        if (!DataManager.Stack.IsEmpty() && DataManager.Stack.Stack[^1] == ViskConsts.I64) stack();
        else if (DataManager.Register.Rx64.CanGetPrevious) reg();
        else if (onError != null) onError();
        else ViskThrowHelper.ThrowInvalidOperationException("Stack and registers are already used");
    }

    private void PreviousStackOrRegD(Action reg, Action stack, Action? onError = null)
    {
        if (!DataManager.Stack.IsEmpty() && DataManager.Stack.Stack[^1] == ViskConsts.F64) stack();
        else if (DataManager.Register.Rd.CanGetPrevious) reg();
        else if (onError != null) onError();
        else ViskThrowHelper.ThrowInvalidOperationException("Stack and registers are already used");
    }

    private void NextStackOrRegX64(Action reg, Action stack)
    {
        if (DataManager.Register.Rx64.CanGetNext) reg();
        else stack();
    }

    private void NextStackOrRegD(Action xmm, Action stack)
    {
        if (DataManager.Register.Rd.CanGetNext) xmm();
        else stack();
    }


    private void CmpValueWithZero()
    {
        PreviousStackOrRegX64(
            () => { DataManager.Assembler.cmp(DataManager.Register.Previous().X64, 0); },
            () =>
            {
                DataManager.Assembler.mov(rax, DataManager.Stack.GetPrevious());
                DataManager.Assembler.cmp(rax, 0);
            }
        );
    }


    private void CmpD(DoubleOperation operation)
    {
        AssemblerRegisterXMM xmm = default;

        OperateD(
            (r, m) => DataManager.Assembler.cmppd(xmm = r, m, (byte)operation),
            false
        );

        NextStackOrRegX64(
            () =>
            {
                var assemblerRegister64 = DataManager.Register.Next(ViskConsts.I64).X64;
                DataManager.Assembler.movq(assemblerRegister64, xmm);
                DataManager.Assembler.and(assemblerRegister64, 1);
            },
            () =>
            {
                var assemblerMemoryOperand = DataManager.Stack.GetNext(ViskConsts.I64);
                DataManager.Assembler.movq(assemblerMemoryOperand, xmm);
                DataManager.Assembler.and(assemblerMemoryOperand, 1);
            });
    }


    private void SaveAllRegisters()
    {
        foreach (var pr in ViskRegister.PublicRegisters)
            DataManager.Assembler.push(pr);
        foreach (var prd in ViskRegister.PublicRegistersD)
        {
            DataManager.Assembler.sub(rsp, 16);
            DataManager.Assembler.movdqu(__[rsp], prd);
        }

        foreach (var pr in ViskLocal.LocalsRegisters)
            DataManager.Assembler.push(pr);
        foreach (var prd in ViskLocal.LocalsRegistersD)
        {
            DataManager.Assembler.sub(rsp, 16);
            DataManager.Assembler.movdqu(__[rsp], prd);
        }

        AlignIfNeed(true);
    }

    private void AlignIfNeed(bool subOrAdd)
    {
        var len = ViskRegister.PublicRegisters.Length + ViskLocal.LocalsRegisters.Length +
                  ViskLocal.LocalsRegistersD.Length + 1;

        if (len % 2 == 0) return;

        if (subOrAdd)
            DataManager.Assembler.sub(rsp, 8);
        else DataManager.Assembler.add(rsp, 8);
    }


    private void LoadAllRegisters()
    {
        AlignIfNeed(false);
        foreach (var prd in ViskLocal.LocalsRegistersD.Reverse())
        {
            DataManager.Assembler.movdqu(prd, __[rsp]);
            DataManager.Assembler.add(rsp, 16);
        }

        foreach (var pr in ViskLocal.LocalsRegisters.Reverse())
            DataManager.Assembler.pop(pr);

        foreach (var prd in ViskRegister.PublicRegistersD.Reverse())
        {
            DataManager.Assembler.movdqu(prd, __[rsp]);
            DataManager.Assembler.add(rsp, 16);
        }

        foreach (var pr in ViskRegister.PublicRegisters.Reverse())
            DataManager.Assembler.pop(pr);
    }
}