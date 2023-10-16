namespace ViskCompiler;

using System.Collections.Immutable;

public sealed record ViskLocal(Type Type, string Name, ViskLocType LocType)
{
    public static readonly ImmutableArray<AssemblerRegister64> LocalsRegisters =
        ImmutableArray.Create(r13, r14, r15);

    public static readonly ImmutableArray<AssemblerRegisterXMM> LocalsRegistersD =
        ImmutableArray.Create(xmm13, xmm14, xmm15);
}