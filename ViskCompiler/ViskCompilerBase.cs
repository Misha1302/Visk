namespace ViskCompiler;

using Iced.Intel;

internal abstract class ViskCompilerBase
{
    private const string NotImplemented = "Not implemented operation";
    protected readonly ViskDataManager DataManager;

    protected ViskCompilerBase(ViskModule module)
    {
        DataManager = new ViskDataManager(new Assembler(64), module);
    }

    public ViskX64AsmExecutor Compile()
    {
        CompileInternal();

        return new ViskX64AsmExecutor(DataManager.Assembler);
    }

    private void CompileInternal()
    {
        CompileFunctions();

        DeclareData();
    }

    private void DeclareData()
    {
        foreach (var data in DataManager.Data)
        {
            var dataItem1 = data.Item1;
            DataManager.Assembler.Label(ref dataItem1);
            DataManager.Assembler.dq(data.Item2);
        }
    }

    private void CompileFunctions()
    {
        var mainFunc = DataManager.Module.Functions.First(x => x.IsMain);

        CompileFunction(mainFunc);

        foreach (var function in DataManager.Module.Functions)
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

        var act = instruction.InstructionKind switch
        {
            ViskInstructionKind.PushConst => PushConst,
            ViskInstructionKind.Add => Add,
            ViskInstructionKind.SetLabel => SetLabel,
            ViskInstructionKind.Goto => Goto,
            ViskInstructionKind.GotoIfNotEquals => GotoIfNotEquals,
            ViskInstructionKind.SetLocal => SetLocal,
            ViskInstructionKind.LoadLocal => LoadLocal,
            ViskInstructionKind.Nop => Nop,
            ViskInstructionKind.Cmp => Cmp,
            ViskInstructionKind.Dup => Dup,
            ViskInstructionKind.IMul => IMul,
            ViskInstructionKind.Sub => Sub,
            ViskInstructionKind.CallForeign => CallForeign,
            ViskInstructionKind.Call => Call,
            ViskInstructionKind.Prolog => Prolog,
            ViskInstructionKind.Drop => Drop,
            ViskInstructionKind.Ret => Ret,
            ViskInstructionKind.SetArg => SetArg,
            _ => ViskThrowHelper.ThrowInvalidOperationException<Action<object?, object?, object?, ViskFunction>>(
                $"Unknown instruction: {instruction}"
            )
        };

        act(arg0, arg1, arg2, func);
    }

    protected virtual void PushConst(object? arg0, object? arg1, object? arg2, ViskFunction func) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Add(object? arg0, object? arg1, object? arg2, ViskFunction func) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Sub(object? arg0, object? arg1, object? arg2, ViskFunction func) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Ret(object? arg0, object? arg1, object? arg2, ViskFunction func) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void CallForeign(object? arg0, object? arg1, object? arg2, ViskFunction func) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    // ReSharper disable once InconsistentNaming
    protected virtual void IMul(object? arg0, object? arg1, object? arg2, ViskFunction func) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void SetLabel(object? arg0, object? arg1, object? arg2, ViskFunction func) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Goto(object? arg0, object? arg1, object? arg2, ViskFunction func) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Prolog(object? arg0, object? arg1, object? arg2, ViskFunction func) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void SetLocal(object? arg0, object? arg1, object? arg2, ViskFunction func) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void LoadLocal(object? arg0, object? arg1, object? arg2, ViskFunction func) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Nop(object? arg0, object? arg1, object? arg2, ViskFunction func) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Call(object? arg0, object? arg1, object? arg2, ViskFunction func) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void GotoIfNotEquals(object? arg0, object? arg1, object? arg2, ViskFunction func) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Dup(object? arg0, object? arg1, object? arg2, ViskFunction func) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Cmp(object? arg0, object? arg1, object? arg2, ViskFunction func) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Drop(object? arg0, object? arg1, object? arg2, ViskFunction func) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void SetArg(object? arg0, object? arg1, object? arg2, ViskFunction func) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    private static void GetArgs(ViskInstruction instruction, out object? o, out object? o1, out object? o2)
    {
        o = instruction.Arguments.Count > 0 ? instruction.Arguments[0] : null;
        o1 = instruction.Arguments.Count > 1 ? instruction.Arguments[1] : null;
        o2 = instruction.Arguments.Count > 2 ? instruction.Arguments[2] : null;
    }
}