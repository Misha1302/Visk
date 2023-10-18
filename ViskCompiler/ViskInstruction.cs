namespace ViskCompiler;

using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;

[DebuggerDisplay("{InstructionKind} {Arguments.Count > 0 ? Arguments[0] : \"\"}")]
public sealed partial class ViskInstruction : IViskAssemblerPositionable
{
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

    [Pure] public static ViskInstruction Ret() => new(ViskInstructionKind.Ret);

    [Pure] public static ViskInstruction RetD() => new(ViskInstructionKind.RetD);

    [Pure] public static ViskInstruction Prolog() => new(ViskInstructionKind.Prolog);

    [Pure] public static ViskInstruction LoadLocal(string name) => new(ViskInstructionKind.LoadLocal, name);

    [Pure] public static ViskInstruction SetLocal(string name) =>
        new(ViskInstructionKind.SetLocal, name, ViskLocType.Default);

    [Pure] public static ViskInstruction Nop() => new(ViskInstructionKind.Nop);

    [Pure] public static ViskInstruction Call(ViskFunction f) => new(ViskInstructionKind.Call, f);

    [Pure] public static ViskInstruction Dup() => new(ViskInstructionKind.Dup);

    [Pure] public static ViskInstruction DupD() => new(ViskInstructionKind.DupD);

    [Pure] public static ViskInstruction Equals() => new(ViskInstructionKind.Equals);

    [Pure] public static ViskInstruction EqualsD() => new(ViskInstructionKind.EqualsD);
    [Pure] public static ViskInstruction NotEqualsD() => new(ViskInstructionKind.NotEqualsD);

    [Pure] public static ViskInstruction Drop() => new(ViskInstructionKind.Drop);

    [Pure] public static ViskInstruction DropD() => new(ViskInstructionKind.DropD);

    [Pure] public static ViskInstruction SetArg(string name) =>
        new(ViskInstructionKind.SetArg, name, ViskLocType.Default);

    [Pure] public static ViskInstruction SetArgD(string name) =>
        new(ViskInstructionKind.SetArgD, name, ViskLocType.Default);

    [Pure] public static ViskInstruction LogicNeg() => new(ViskInstructionKind.LogicNeg);

    // ReSharper disable once InconsistentNaming
    [Pure] public static ViskInstruction IDiv() => new(ViskInstructionKind.IDiv);
    [Pure] public static ViskInstruction NotEquals() => new(ViskInstructionKind.NotEquals);

    [Pure] public static ViskInstruction PushConstD(double d) => new(ViskInstructionKind.PushConstD, d);

    [Pure] public static ViskInstruction SetLocalD(string s) =>
        new(ViskInstructionKind.SetLocalD, s, ViskLocType.Default);

    [Pure] public static ViskInstruction LoadLocalD(string s) => new(ViskInstructionKind.LoadLocalD, s);

    [Pure] public static ViskInstruction LessThan() => new(ViskInstructionKind.LessThan);
    [Pure] public static ViskInstruction LessThanOrEquals() => new(ViskInstructionKind.LessThanOrEquals);
    [Pure] public static ViskInstruction GreaterThan() => new(ViskInstructionKind.GreaterThan);
    [Pure] public static ViskInstruction GreaterThanOrEquals() => new(ViskInstructionKind.GreaterThanOrEquals);

    [Pure]
    public static ViskInstruction IfTrue(List<ViskInstruction> ifBlock, List<ViskInstruction>? elseBlock = null) =>
        new(ViskInstructionKind.IfTrue, ifBlock, elseBlock ?? new List<ViskInstruction>());

    [Pure] public static ViskInstruction IfTrue(params ViskInstruction[] ifBlock) => IfTrue(ifBlock.ToList());

    [Pure]
    public static ViskInstruction IfFalse(List<ViskInstruction> ifBlock, List<ViskInstruction>? elseBlock = null) =>
        new(ViskInstructionKind.IfFalse, ifBlock, elseBlock ?? new List<ViskInstruction>());

    [Pure] public static ViskInstruction IfFalse(params ViskInstruction[] ifBlock) => IfFalse(ifBlock.ToList());

    [Pure] public static ViskInstruction LessThanD() => new(ViskInstructionKind.LessThanD);
    [Pure] public static ViskInstruction GreaterThanD() => new(ViskInstructionKind.GreaterThanD);
    [Pure] public static ViskInstruction LessThanOrEqualsD() => new(ViskInstructionKind.LessThanOrEqualsD);
    [Pure] public static ViskInstruction GreaterThanOrEqualsD() => new(ViskInstructionKind.GreaterThanOrEqualsD);

    [Pure] public static ViskInstruction LoadRef(string s) => new(ViskInstructionKind.LoadRef, s);

    [Pure] public static ViskInstruction SetByRef(int i) => new(ViskInstructionKind.SetByRef, i);

    [Pure] public static ViskInstruction SetByRefD() => new(ViskInstructionKind.SetByRefD);

    [Pure] public static ViskInstruction LoadByRef() => new(ViskInstructionKind.LoadByRef);
    [Pure] public static ViskInstruction LoadByRefD() => new(ViskInstructionKind.LoadByRefD);

    [Pure] public static ViskInstruction CallBuildIn(ViskBuildIn func) => new(ViskInstructionKind.CallBuildIn, func);
}