namespace ViskCompiler;

using System.Collections.Immutable;
using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal sealed class ViskRegister
{
    public static readonly ImmutableArray<AssemblerRegister64> Registers =
        ImmutableArray.Create(rbx, r10, r11, r12, r13, r14, r15);

    public static readonly ImmutableArray<AssemblerRegister8> Registers8 =
        ImmutableArray.Create(bl, r10b, r11b, r12b, r13b, r14b, r15b);

    public int CurIndex { get; private set; }

    public bool CanGetPrevious => CurIndex - 1 >= 0;
    public bool CanGetNext => CurIndex < Registers.Length;


    public AssemblerRegister64 Next() =>
        Registers[
            CurIndex >= Registers.Length
                ? ViskThrowHelper.ThrowInvalidOperationException<int>("All registers are used")
                : CurIndex++
        ];

    public AssemblerRegister64 Previous() =>
        Registers[
            CurIndex - 1 < 0
                ? ViskThrowHelper.ThrowInvalidOperationException<int>("No register to return")
                : --CurIndex
        ];

    public AssemblerRegister8 Next8() =>
        Registers8[
            CurIndex >= Registers8.Length
                ? ViskThrowHelper.ThrowInvalidOperationException<int>("All registers are used")
                : CurIndex++
        ];

    public AssemblerRegister8 Previous8() =>
        Registers8[
            CurIndex - 1 < 0
                ? ViskThrowHelper.ThrowInvalidOperationException<int>("No register to return")
                : --CurIndex
        ];

    public void Reset() => CurIndex = 0;

    public AssemblerRegister64 BackValue() => Registers[
        CurIndex - 1 < 0
            ? ViskThrowHelper.ThrowInvalidOperationException<int>("No register to return")
            : CurIndex - 1
    ];

    public AssemblerRegister8 Current8() => Registers8[CurIndex];

    public AssemblerRegister64 Current() => Registers[CurIndex];
}