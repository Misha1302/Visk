﻿namespace ViskCompiler;

using System.Collections.Immutable;
using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal sealed class ViskRegister
{
    public static readonly ImmutableArray<AssemblerRegister64> Registers =
        ImmutableArray.Create(rbx, r10, r11, r12);

    public ViskRegister()
    {
    }

    public ViskRegister(int value)
    {
        CurIndex = value;
    }

    public int CurIndex { get; private set; }

    public bool CanGetPrevious => CurIndex - 1 >= 0;
    public bool CanGetNext => CurIndex < Registers.Length;


    /// <summary>
    /// </summary>
    /// <returns>old value</returns>
    public AssemblerRegister64 Next() =>
        Registers[
            CurIndex >= Registers.Length
                ? throw new InvalidOperationException("All registers are used")
                : CurIndex++
        ];

    /// <summary>
    /// </summary>
    /// <returns>old value</returns>
    public AssemblerRegister64 Previous() =>
        Registers[
            CurIndex - 1 < 0
                ? throw new InvalidOperationException("No register to return")
                : --CurIndex
        ];

    public void Reset() => CurIndex = 0;

    public void Sub(int argsCount)
    {
        var predict = CurIndex - argsCount;
        if (predict < 0 || predict >= Registers.Length)
            throw new InvalidOperationException();

        CurIndex = predict;
    }

    public AssemblerRegister64 BackValue() => Registers[
        CurIndex - 1 < 0
            ? throw new InvalidOperationException("No register to return")
            : CurIndex - 1
    ];
}