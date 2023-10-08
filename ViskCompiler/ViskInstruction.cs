namespace ViskCompiler;

using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;

[DebuggerDisplay("{InstructionKind} {Arguments.Count > 0 ? Arguments[0] : \"\"}")]
public sealed class ViskInstruction
{
    public static readonly Dictionary<ViskInstructionKind, (int args, int output)> InstructionCharacteristics =
        new()
        {
            [ViskInstructionKind.PushConst] = (0, 1),
            [ViskInstructionKind.LogicNeg] = (1, 1),
            [ViskInstructionKind.Add] = (2, 1),
            [ViskInstructionKind.Sub] = (2, 1),
            [ViskInstructionKind.Ret] = (1, 0),
            [ViskInstructionKind.Equals] = (2, 1),
            [ViskInstructionKind.NotEquals] = (2, 1),
            [ViskInstructionKind.IDiv] = (2, 1),
            [ViskInstructionKind.SetArg] = (1, 0),
            [ViskInstructionKind.CallForeign] = (100_000, 100_000),
            [ViskInstructionKind.Call] = (100_000, 100_000),
            [ViskInstructionKind.IMul] = (2, 1),
            [ViskInstructionKind.SetLabel] = (0, 0),
            [ViskInstructionKind.Goto] = (0, 0),
            [ViskInstructionKind.Drop] = (1, 0),
            [ViskInstructionKind.GotoIfFalse] = (1, 0),
            [ViskInstructionKind.GotoIfTrue] = (1, 0),
            [ViskInstructionKind.Prolog] = (0, 0),
            [ViskInstructionKind.SetLocal] = (0, 0),
            [ViskInstructionKind.LoadLocal] = (0, 1),
            [ViskInstructionKind.Dup] = (1, 2),
            [ViskInstructionKind.Nop] = (0, 0)
        };

    public readonly List<object> Arguments;

    private ViskInstruction(ViskInstructionKind instructionKind, params object[]? arguments)
    {
        InstructionKind = instructionKind;
        Arguments = new List<object>(arguments ?? Array.Empty<object>());
    }

    public ViskInstructionKind InstructionKind { get; }


    [Pure] public static ViskInstruction PushConst(int n) => new(ViskInstructionKind.PushConst, n);

    [Pure] public static ViskInstruction CallForeign(MethodInfo? m)
    {
        if (m is null)
            ViskThrowHelper.ThrowInvalidOperationException();

        RuntimeHelpers.PrepareMethod(m.MethodHandle);

        return new ViskInstruction(
            ViskInstructionKind.CallForeign,
            m.MethodHandle.GetFunctionPointer(), m.GetParameters().Length, m.ReturnType
        );
    }

    [Pure] public static ViskInstruction Add() => new(ViskInstructionKind.Add);
    [Pure] public static ViskInstruction Sub() => new(ViskInstructionKind.Sub);

    // ReSharper disable once InconsistentNaming
    [Pure] public static ViskInstruction IMul() => new(ViskInstructionKind.IMul);

    [Pure] public static ViskInstruction SetLabel(string label) => new(ViskInstructionKind.SetLabel, label);

    [Pure] public static ViskInstruction Goto(string label) => new(ViskInstructionKind.Goto, label);

    [Pure] public static ViskInstruction GotoIfFalse(string label) =>
        new(ViskInstructionKind.GotoIfFalse, label);

    [Pure] public static ViskInstruction GotoIfTrue(string label) =>
        new(ViskInstructionKind.GotoIfTrue, label);

    [Pure] public static ViskInstruction Ret() => new(ViskInstructionKind.Ret);

    [Pure] public static ViskInstruction Prolog() => new(ViskInstructionKind.Prolog);

    [Pure] public static ViskInstruction LoadLocal(string name) => new(ViskInstructionKind.LoadLocal, name);

    [Pure] public static ViskInstruction SetLocal(string name) => new(ViskInstructionKind.SetLocal, name);

    [Pure] public static ViskInstruction Nop() => new(ViskInstructionKind.Nop);

    [Pure] public static ViskInstruction Call(ViskFunction f) => new(ViskInstructionKind.Call, f);

    [Pure] public static ViskInstruction Dup() => new(ViskInstructionKind.Dup);

    [Pure] public static ViskInstruction Equals() => new(ViskInstructionKind.Equals);

    [Pure] public static ViskInstruction Drop() => new(ViskInstructionKind.Drop);

    [Pure] public static ViskInstruction SetArg(string name) => new(ViskInstructionKind.SetArg, name, -1);

    [Pure] public static ViskInstruction LogicNeg() => new(ViskInstructionKind.LogicNeg);

    // ReSharper disable once InconsistentNaming
    [Pure] public static ViskInstruction IDiv() => new(ViskInstructionKind.IDiv);
    [Pure] public static ViskInstruction NotEquals() => new(ViskInstructionKind.NotEquals);
}