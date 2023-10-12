﻿namespace ViskCompiler;

using System.Diagnostics.Contracts;
using Iced.Intel;

internal abstract class ViskCompilerBase
{
    private const string NotImplemented = "Not implemented operation";
    protected readonly ViskDataManager DataManager;

    protected ViskCompilerBase(ViskModule module, ViskSettings settings)
    {
        DataManager = new ViskDataManager(new Assembler(64), module, settings);
    }

    public ViskX64AsmExecutor Compile()
    {
        CompileInternal();

        return new ViskX64AsmExecutor(DataManager.Assembler, DataManager.DebugInfo);
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
        var mainFunc = DataManager.Module.Functions.FirstOrDefault(x => x.Info.IsMain);

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
        func.Info.AssemblerInstructionIndex = DataManager.Assembler.Instructions.Count;
        DataManager.DebugInfo.AddFunction(func.Info);

        foreach (var instruction in func.TotalInstructions)
            CompileInstruction(instruction, func);
    }

    private void CompileInstruction(ViskInstruction instruction, ViskFunction func)
    {
        var args = GetArgs(instruction, func);

        instruction.AssemblerInstructionIndex = DataManager.Assembler.Instructions.Count;
        DataManager.DebugInfo.AddInstruction(instruction);

        var act = instruction.InstructionKind switch
        {
            ViskInstructionKind.PushConst => PushConst,
            ViskInstructionKind.Add => Add,
            ViskInstructionKind.SetLabel => SetLabel,
            ViskInstructionKind.Goto => Goto,
            ViskInstructionKind.GotoIfFalse => GotoIfFalse,
            ViskInstructionKind.SetLocal => SetLocal,
            ViskInstructionKind.LoadLocal => LoadLocal,
            ViskInstructionKind.Nop => Nop,
            ViskInstructionKind.Dup => Dup,
            ViskInstructionKind.IMul => IMul,
            ViskInstructionKind.IDiv => IDiv,
            ViskInstructionKind.Sub => Sub,
            ViskInstructionKind.Equals => Equals,
            ViskInstructionKind.NotEquals => NotEquals,
            ViskInstructionKind.CallForeign => CallForeign,
            ViskInstructionKind.Call => Call,
            ViskInstructionKind.Prolog => Prolog,
            ViskInstructionKind.Drop => Drop,
            ViskInstructionKind.Ret => Ret,
            ViskInstructionKind.SetArg => SetArg,
            ViskInstructionKind.LogicNeg => LogicNeg,
            ViskInstructionKind.GotoIfTrue => GotoIfTrue,
            ViskInstructionKind.PushConstD => PushConstD,
            _ => ViskThrowHelper.ThrowInvalidOperationException<Action<InstructionArgs>>(
                $"Unknown instruction: {instruction}"
            )
        };

        act(args);
    }

    protected virtual void PushConst(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);
    
    protected virtual void PushConstD(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void GotoIfTrue(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Add(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Sub(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    // ReSharper disable once InconsistentNaming
    protected virtual void IDiv(InstructionArgs args) =>
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

    protected virtual void GotoIfFalse(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Dup(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void Equals(InstructionArgs args) =>
        ViskThrowHelper.ThrowInvalidOperationException(NotImplemented);

    protected virtual void NotEquals(InstructionArgs args) =>
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