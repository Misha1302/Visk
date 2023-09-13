namespace ViskCompiler;

using System.Reflection;
using System.Runtime.CompilerServices;

public sealed class ViskInstruction
{
    private ViskInstruction(ViskInstructionKind instructionKind, params object[]? arguments)
    {
        InstructionKind = instructionKind;
        Arguments = arguments;
    }

    public ViskInstructionKind InstructionKind { get; }
    public object[]? Arguments { get; }


    public static ViskInstruction PushConst(int n) => new(ViskInstructionKind.PushConst, n);

    public static ViskInstruction CallForeign(MethodInfo? m, int argsCount, ViskType returnType)
    {
        if (m is null)
            throw new InvalidOperationException();

        RuntimeHelpers.PrepareMethod(m.MethodHandle);

        return new ViskInstruction(
            ViskInstructionKind.CallForeign,
            m.MethodHandle.GetFunctionPointer(), argsCount, returnType
        );
    }

    public static ViskInstruction Add() => new(ViskInstructionKind.Add);

    public static ViskInstruction CSharpRet() => new(ViskInstructionKind.CSharpRet);

    // ReSharper disable once InconsistentNaming
    public static ViskInstruction IMul() => new(ViskInstructionKind.IMul);
}