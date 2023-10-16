namespace ViskCompiler;

using System.Collections.Immutable;

public sealed record ViskLocal(Type Type, string Name)
{
    public static readonly ImmutableArray<AssemblerRegister64> LocalsRegisters =
        ImmutableArray.Create<AssemblerRegister64>();

    public static readonly ImmutableArray<AssemblerRegisterXMM> LocalsRegistersD =
        ImmutableArray.Create<AssemblerRegisterXMM>();
}