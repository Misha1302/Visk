namespace ViskCompiler;

using System.Collections.Immutable;
using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal sealed class ViskRegister
{
    public static readonly ImmutableArray<AssemblerRegister64> PublicRegisters =
        ImmutableArray.Create(r10, r11, r12, rbx, rsi);

    public static readonly ImmutableArray<AssemblerRegisterXMM> PublicRegistersD =
        ImmutableArray.Create(xmm4, xmm5, xmm6, xmm7, xmm8, xmm9, xmm10, xmm11, xmm12);

    public static readonly IReadOnlyDictionary<AssemblerRegister64, AssemblerRegister8> PublicRegisters8 =
        new Dictionary<AssemblerRegister64, AssemblerRegister8>
        {
            [r10] = r10b,
            [r11] = r11b,
            [r12] = r12b,
            [rbx] = bl,
            [rsi] = sil
        };

    public readonly ViskRegisterInternal<AssemblerRegister64> Rx64 = new(PublicRegisters);
    public readonly ViskRegisterInternal<AssemblerRegisterXMM> Rd = new(PublicRegistersD);
}