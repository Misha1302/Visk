namespace ViskCompiler;

using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;

[DebuggerDisplay("{InstructionKind} {Arguments.Count > 0 ? Arguments[0] : \"\"}")]
public sealed class ViskInstruction : IViskAssemblerPositionable
{
    public static readonly Dictionary<ViskInstructionKind, (int args, int output)> InstructionCharacteristics =
        new()
        {
            [ViskInstructionKind.PushConst] = (0, 1),
            [ViskInstructionKind.PushConstD] = (0, 1),
            [ViskInstructionKind.LogicNeg] = (1, 1),
            [ViskInstructionKind.Add] = (2, 1),
            [ViskInstructionKind.AddD] = (2, 1),
            [ViskInstructionKind.SubD] = (2, 1),
            [ViskInstructionKind.MulD] = (2, 1),
            [ViskInstructionKind.DivD] = (2, 1),
            [ViskInstructionKind.DupD] = (2, 1),
            [ViskInstructionKind.DropD] = (2, 1),
            [ViskInstructionKind.Sub] = (2, 1),
            [ViskInstructionKind.Ret] = (1, 0),
            [ViskInstructionKind.RetD] = (1, 0),
            [ViskInstructionKind.Equals] = (2, 1),
            [ViskInstructionKind.LessThan] = (2, 1),
            [ViskInstructionKind.GreaterThan] = (2, 1),
            [ViskInstructionKind.LessThanOrEquals] = (2, 1),
            [ViskInstructionKind.GreaterThanOrEquals] = (2, 1),
            [ViskInstructionKind.NotEquals] = (2, 1),
            [ViskInstructionKind.IDiv] = (2, 1),
            [ViskInstructionKind.SetArg] = (1, 0),
            [ViskInstructionKind.SetArgD] = (1, 0),
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
            [ViskInstructionKind.SetLocalD] = (0, 0),
            [ViskInstructionKind.LoadLocal] = (0, 1),
            [ViskInstructionKind.LoadLocalD] = (0, 1),
            [ViskInstructionKind.Dup] = (1, 2),
            [ViskInstructionKind.Nop] = (0, 0)
        };

    public readonly List<object> Arguments;
    public readonly ViskInstructionKind InstructionKind;

    private ViskInstruction(ViskInstructionKind instructionKind, params object[]? arguments)
    {
        InstructionKind = instructionKind;
        Arguments = new List<object>(arguments ?? Array.Empty<object>());
    }

    public int AssemblerInstructionIndex { get; set; }

    public override string ToString() =>
        $"{InstructionKind} {string.Join(", ", Arguments.Select(x => x.ToStringRecursive()))}";

    [Pure] public static ViskInstruction CallForeign(MethodInfo? m)
    {
        if (m is null)
            ViskThrowHelper.ThrowInvalidOperationException();

        RuntimeHelpers.PrepareMethod(m.MethodHandle);

        // m.Name - debug info
        return new ViskInstruction(
            ViskInstructionKind.CallForeign,
            m.MethodHandle.GetFunctionPointer(),
            m.GetParameters().Select(x => x.ParameterType).ToList(),
            m.ReturnType,
            m.Name
        );
    }

    [Pure] public static ViskInstruction PushConst(long n) => new(ViskInstructionKind.PushConst, n);

    [Pure] public static ViskInstruction Add() => new(ViskInstructionKind.Add);

    [Pure] public static ViskInstruction AddD() => new(ViskInstructionKind.AddD);

    [Pure] public static ViskInstruction SubD() => new(ViskInstructionKind.SubD);

    [Pure] public static ViskInstruction MulD() => new(ViskInstructionKind.MulD);

    [Pure] public static ViskInstruction DivD() => new(ViskInstructionKind.DivD);

    [Pure] public static ViskInstruction Sub() => new(ViskInstructionKind.Sub);

    // ReSharper disable once InconsistentNaming
    [Pure] public static ViskInstruction IMul() => new(ViskInstructionKind.IMul);

    [Pure] public static ViskInstruction SetLabel(string label) => new(ViskInstructionKind.SetLabel, label);

    [Pure] public static ViskInstruction Goto(string label) => new(ViskInstructionKind.Goto, label);

    [Pure] public static ViskInstruction GotoIfFalse(string label) => new(ViskInstructionKind.GotoIfFalse, label);

    [Pure] public static ViskInstruction GotoIfTrue(string label) => new(ViskInstructionKind.GotoIfTrue, label);

    [Pure] public static ViskInstruction Ret() => new(ViskInstructionKind.Ret);

    [Pure] public static ViskInstruction RetD() => new(ViskInstructionKind.RetD);

    [Pure] public static ViskInstruction Prolog() => new(ViskInstructionKind.Prolog);

    [Pure] public static ViskInstruction LoadLocal(string name) => new(ViskInstructionKind.LoadLocal, name);

    [Pure] public static ViskInstruction SetLocal(string name) => new(ViskInstructionKind.SetLocal, name);

    [Pure] public static ViskInstruction Nop() => new(ViskInstructionKind.Nop);

    [Pure] public static ViskInstruction Call(ViskFunction f) => new(ViskInstructionKind.Call, f);

    [Pure] public static ViskInstruction Dup() => new(ViskInstructionKind.Dup);

    [Pure] public static ViskInstruction DupD() => new(ViskInstructionKind.DupD);

    [Pure] public static ViskInstruction Equals() => new(ViskInstructionKind.Equals);

    [Pure] public static ViskInstruction Drop() => new(ViskInstructionKind.Drop);

    [Pure] public static ViskInstruction DropD() => new(ViskInstructionKind.DropD);

    [Pure] public static ViskInstruction SetArg(string name) => new(ViskInstructionKind.SetArg, name);

    [Pure] public static ViskInstruction SetArgD(string name) => new(ViskInstructionKind.SetArgD, name);

    [Pure] public static ViskInstruction LogicNeg() => new(ViskInstructionKind.LogicNeg);

    // ReSharper disable once InconsistentNaming
    [Pure] public static ViskInstruction IDiv() => new(ViskInstructionKind.IDiv);
    [Pure] public static ViskInstruction NotEquals() => new(ViskInstructionKind.NotEquals);

    [Pure] public static ViskInstruction PushConstD(double d) => new(ViskInstructionKind.PushConstD, d);

    [Pure] public static ViskInstruction SetLocalD(string s) => new(ViskInstructionKind.SetLocalD, s);

    [Pure] public static ViskInstruction LoadLocalD(string s) => new(ViskInstructionKind.LoadLocalD, s);

    [Pure] public static ViskInstruction LessThan() => new(ViskInstructionKind.LessThan);
    [Pure] public static ViskInstruction LessThanOrEquals() => new(ViskInstructionKind.LessThanOrEquals);
    [Pure] public static ViskInstruction GreaterThan() => new(ViskInstructionKind.GreaterThan);
    [Pure] public static ViskInstruction GreaterThanOrEquals() => new(ViskInstructionKind.GreaterThanOrEquals);
}