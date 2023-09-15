namespace ViskCompiler;

using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal sealed class ViskRegister
{
    private static readonly AssemblerRegister64[] _registers =
    {
        rbx, r10, r11, r12, r13, r14, r15
    };

    private int _curRegister;

    public ViskRegister()
    {
    }

    public ViskRegister(AssemblerRegister64 value)
    {
        _curRegister = Array.IndexOf(_registers, value);
    }

    public AssemblerRegister64 CurValue => _registers[_curRegister];


    /// <summary>
    /// </summary>
    /// <returns>old value</returns>
    public AssemblerRegister64 Next() =>
        _registers[
            _curRegister >= _registers.Length
                ? throw new InvalidOperationException("All registers are used")
                : _curRegister++
        ];

    /// <summary>
    /// </summary>
    /// <returns>old value</returns>
    public AssemblerRegister64 Previous() =>
        _registers[
            _curRegister - 1 < 0
                ? throw new InvalidOperationException("No register to return")
                : --_curRegister
        ];

    public void Reset() => _curRegister = 0;
}