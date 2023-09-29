namespace ViskCompiler;

using System.Collections.Immutable;
using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal sealed class ViskRegister
{
    public static readonly ImmutableArray<AssemblerRegister64> Registers =
        ImmutableArray.Create(rbx, r10, r11, r12, r13);

    private int _curRegister;

    public ViskRegister()
    {
    }

    public ViskRegister(AssemblerRegister64 value)
    {
        _curRegister = Registers.IndexOf(value);
    }

    public AssemblerRegister64 CurValue => Registers[_curRegister];
    public bool CanGetPrevious => _curRegister - 1 >= 0;


    /// <summary>
    /// </summary>
    /// <returns>old value</returns>
    public AssemblerRegister64 Next() =>
        Registers[
            _curRegister >= Registers.Length
                ? throw new InvalidOperationException("All registers are used")
                : _curRegister++
        ];

    /// <summary>
    /// </summary>
    /// <returns>old value</returns>
    public AssemblerRegister64 Previous() =>
        Registers[
            _curRegister - 1 < 0
                ? throw new InvalidOperationException("No register to return")
                : --_curRegister
        ];

    public void Reset() => _curRegister = 0;

    public void Sub(int argsCount)
    {
        var predict = _curRegister - argsCount;
        if (predict < 0 || predict >= Registers.Length)
            throw new InvalidOperationException();

        _curRegister = predict;
    }

    public AssemblerRegister64 BackValue() => Registers[
        _curRegister - 1 < 0
            ? throw new InvalidOperationException("No register to return")
            : _curRegister - 1
    ];
}