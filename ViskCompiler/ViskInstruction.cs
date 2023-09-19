namespace ViskCompiler;

using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

[DebuggerDisplay("{InstructionKind} {Arguments.Count > 0 ? Arguments[0] : \"\"}")]
public sealed class ViskInstruction
{
    public readonly List<object> Arguments;

    private ViskInstruction(ViskInstructionKind instructionKind, params object[]? arguments)
    {
        InstructionKind = instructionKind;
        Arguments = new List<object>(arguments ?? Array.Empty<object>());
    }

    public ViskInstructionKind InstructionKind { get; }


    public static ViskInstruction PushConst(int n) => new(ViskInstructionKind.PushConst, n);

    public static ViskInstruction CallForeign(MethodInfo? m)
    {
        if (m is null)
            throw new InvalidOperationException();

        RuntimeHelpers.PrepareMethod(m.MethodHandle);

        return new ViskInstruction(
            ViskInstructionKind.CallForeign,
            m.MethodHandle.GetFunctionPointer(), m.GetParameters().Length
        );
    }

    public static ViskInstruction Add() => new(ViskInstructionKind.Add);

    // ReSharper disable once InconsistentNaming
    public static ViskInstruction IMul() => new(ViskInstructionKind.IMul);

    public static ViskInstruction SetLabel(string label) => new(ViskInstructionKind.SetLabel, label);

    public static ViskInstruction Goto(string label) => new(ViskInstructionKind.Goto, label);

    public static ViskInstruction Ret() => new(ViskInstructionKind.Ret);

    public static ViskInstruction Prolog(int localsSize) => new(ViskInstructionKind.Prolog, localsSize);

    public static ViskInstruction LoadLocal(string name) => new(ViskInstructionKind.LoadLocal, name);

    public static ViskInstruction SetLocal(string name) => new(ViskInstructionKind.SetLocal, name);

    public static ViskInstruction Nop() => new(ViskInstructionKind.Nop);
}