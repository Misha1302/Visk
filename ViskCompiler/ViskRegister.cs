namespace ViskCompiler;

using System.Collections.Immutable;
using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal sealed class ViskRegister
{
    public static readonly ImmutableArray<AssemblerRegister64> PublicRegisters =
        ImmutableArray.Create(rbx, r10, r11, r12);

    public static readonly ImmutableArray<AssemblerRegister64> LocalsRegisters =
        ImmutableArray.Create(r13, r14, r15);

    private static readonly ImmutableArray<AssemblerRegister8> _publicRegisters8 =
        ImmutableArray.Create(bl, r10b, r11b, r12b);

    public int CurIndex { get; private set; }

    public bool CanGetPrevious => CurIndex - 1 >= 0;
    public bool CanGetNext => CurIndex < PublicRegisters.Length;


    public AssemblerRegister64 Next() =>
        PublicRegisters[
            CurIndex >= PublicRegisters.Length
                ? ViskThrowHelper.ThrowInvalidOperationException<int>("All registers are used")
                : CurIndex++
        ];

    public AssemblerRegister64 Previous() =>
        PublicRegisters[
            CurIndex - 1 < 0
                ? ViskThrowHelper.ThrowInvalidOperationException<int>("No register to return")
                : --CurIndex
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

    public void Reset() => CurIndex = 0;

    public AssemblerRegister64 BackValue() => PublicRegisters[
        CurIndex - 1 < 0
            ? ViskThrowHelper.ThrowInvalidOperationException<int>("No register to return")
            : CurIndex - 1
    ];

    public AssemblerRegister8 Current8() => _publicRegisters8[CurIndex];

    public AssemblerRegister64 Current() => PublicRegisters[CurIndex];
}