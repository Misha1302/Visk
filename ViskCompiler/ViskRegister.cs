namespace ViskCompiler;

using System.Collections.Immutable;
using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal sealed class ViskRegister
{
    public static readonly ImmutableArray<AssemblerRegister64> PublicRegisters =
        ImmutableArray.Create(r10, r11, r12, rbx, rsi);

    public static readonly ImmutableArray<AssemblerRegister64> LocalsRegisters =
        ImmutableArray.Create(r13, r14, r15);

    private static readonly ImmutableArray<AssemblerRegister8> _publicRegisters8 =
        ImmutableArray.Create(r10b, r11b, r12b, bl, sil);

    public static readonly ImmutableArray<AssemblerRegisterXMM> PublicRegistersD =
        ImmutableArray.Create(xmm4, xmm5, xmm6, xmm7, xmm8, xmm9, xmm10, xmm11, xmm12);

    public static readonly ImmutableArray<AssemblerRegisterXMM> LocalsRegistersD =
        ImmutableArray.Create(xmm13, xmm14, xmm15);

    public int CurIndex { get; private set; }

    public bool CanGetPrevious => CurIndex - 1 >= 0;
    public bool CanGetNext => CurIndex < PublicRegisters.Length;

    public int CurIndexD { get; private set; }

    public bool CanGetPreviousD => CurIndexD - 1 >= 0;
    public bool CanGetNextD => CurIndexD < PublicRegisters.Length;


    public AssemblerRegister64 Next() =>
        PublicRegisters[
            CurIndex >= PublicRegisters.Length
                ? ViskThrowHelper.ThrowInvalidOperationException<int>("All registers are used")
                : CurIndex++
        ];

    public AssemblerRegisterXMM NextD() =>
        PublicRegistersD[
            CurIndex >= PublicRegistersD.Length
                ? ViskThrowHelper.ThrowInvalidOperationException<int>("All registers are used")
                : CurIndexD++
        ];

    public AssemblerRegister64 Previous() =>
        PublicRegisters[
            CurIndex - 1 < 0
                ? ViskThrowHelper.ThrowInvalidOperationException<int>("No register to return")
                : --CurIndex
        ];

    public AssemblerRegisterXMM PreviousD() =>
        PublicRegistersD[
            CurIndexD - 1 < 0
                ? ViskThrowHelper.ThrowInvalidOperationException<int>("No register to return")
                : --CurIndexD
        ];

    public AssemblerRegister8 Next8() =>
        _publicRegisters8[
            CurIndex >= _publicRegisters8.Length
                ? ViskThrowHelper.ThrowInvalidOperationException<int>("All registers are used")
                : CurIndex++
        ];

    public AssemblerRegister8 Previous8() =>
        _publicRegisters8[
            CurIndex - 1 < 0
                ? ViskThrowHelper.ThrowInvalidOperationException<int>("No register to return")
                : --CurIndex
        ];

    public void Reset() => CurIndex = CurIndexD = 0;

    public AssemblerRegister64 BackValue() => PublicRegisters[
        CurIndex - 1 < 0
            ? ViskThrowHelper.ThrowInvalidOperationException<int>("No register to return")
            : CurIndex - 1
    ];

    public AssemblerRegisterXMM BackValueD() => PublicRegistersD[
        CurIndexD - 1 < 0
            ? ViskThrowHelper.ThrowInvalidOperationException<int>("No register to return")
            : CurIndexD - 1
    ];

    public AssemblerRegister8 Current8() => _publicRegisters8[CurIndex];

    public AssemblerRegister64 Current() => PublicRegisters[CurIndex];
    public AssemblerRegisterXMM CurrentD() => PublicRegistersD[CurIndexD];
}