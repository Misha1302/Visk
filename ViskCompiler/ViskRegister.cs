﻿namespace ViskCompiler;

using System.Collections.Immutable;

internal sealed class ViskRegister
{
    public static readonly ImmutableArray<AssemblerRegister64> PublicRegisters =
        ImmutableArray.Create(rcx, rbx, r8, r9, r10, r11, r12, rsi);

    public static readonly ImmutableArray<AssemblerRegisterXMM> PublicRegistersD =
        ImmutableArray.Create(xmm2, xmm3, xmm4, xmm5, xmm6, xmm7, xmm8, xmm9, xmm10, xmm11, xmm12);

    public static readonly IReadOnlyDictionary<AssemblerRegister64, AssemblerRegister8> PublicRegisters8 =
        new Dictionary<AssemblerRegister64, AssemblerRegister8>
        {
            [rcx] = cl,
            [r8] = r8b,
            [r9] = r9b,
            [r10] = r10b,
            [r11] = r11b,
            [r12] = r12b,
            [rbx] = bl,
            [rsi] = sil
        };

    public static readonly IReadOnlyDictionary<AssemblerRegister64, AssemblerRegister16> PublicRegisters16 =
        new Dictionary<AssemblerRegister64, AssemblerRegister16>
        {
            [rcx] = cx,
            [r8] = r8w,
            [r9] = r9w,
            [r10] = r10w,
            [r11] = r11w,
            [r12] = r12w,
            [rbx] = bx,
            [rsi] = si
        };

    private readonly Stack<Type> _stack = new();

    public readonly ViskRegisterInternal<AssemblerRegister64> Rx64 = new(PublicRegisters);
    public readonly ViskRegisterInternal<AssemblerRegisterXMM> Rd = new(PublicRegistersD);

    private ViskRegister(ViskRegisterInternal<AssemblerRegister64> rx64, ViskRegisterInternal<AssemblerRegisterXMM> rd)
    {
        Rx64 = rx64.Copy();
        Rd = rd.Copy();
    }

    public ViskRegister()
    {
    }

    public ViskRegistersPair Next(Type t)
    {
        _stack.Push(t);
        return RegisterOp(t, Rx64.Next, Rd.Next);
    }

    public ViskRegistersPair Previous() =>
        RegisterOp(_stack.Pop(), Rx64.Previous, Rd.Previous);

    public ViskRegistersPair BackValue() =>
        RegisterOp(_stack.Peek(), Rx64.BackValue, Rd.BackValue);


    private static ViskRegistersPair RegisterOp(
        Type t,
        Func<AssemblerRegister64> actX64,
        Func<AssemblerRegisterXMM> actXmm
    )
    {
        var x64 = default(AssemblerRegister64);
        var d = default(AssemblerRegisterXMM);

        t.ChooseViskType(
            () => x64 = actX64(),
            () => d = actXmm()
        );

        return new ViskRegistersPair(x64, d);
    }

    public ViskRegister Copy() => new(Rx64, Rd);
}