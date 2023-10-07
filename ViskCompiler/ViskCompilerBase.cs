namespace ViskCompiler;

using System.Diagnostics.Contracts;
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
        var mainFunc = DataManager.Module.Functions.FirstOrDefault(x => x.IsMain);

        if (mainFunc != null)
            CompileFunction(mainFunc);
        else throw new InvalidOperationException("The main func was not found");

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
        var args = GetArgs(instruction, func);

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
            ViskInstructionKind.LogicNeg => LogicNeg,
            _ => ViskThrowHelper.ThrowInvalidOperationException<Action<InstructionArgs>>(
                $"Unknown instruction: {instruction}"
            )
        };

        act(args);
    }

    protected virtual void PushConst(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Add(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Sub(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Ret(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void CallForeign(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    // ReSharper disable once InconsistentNaming
    protected virtual void IMul(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void SetLabel(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Goto(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Prolog(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void SetLocal(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void LoadLocal(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Nop(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Call(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void GotoIfNotEquals(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Dup(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Cmp(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Drop(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void SetArg(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    [Pure]
    private static InstructionArgs GetArgs(ViskInstruction instruction, ViskFunction func) =>
        new(instruction.Arguments, func);

    protected virtual void LogicNeg(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected readonly struct InstructionArgs
    {
        private readonly IReadOnlyList<object?> _args;
        public readonly ViskFunction Function;


        public object this[int index] =>
            _args[index] ?? ViskThrowHelper.ThrowInvalidOperationException("Argument is null");

        public InstructionArgs(IReadOnlyList<object?> args, ViskFunction function)
        {
            _args = args;
            Function = function;
        }
    }
}