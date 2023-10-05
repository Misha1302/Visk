namespace ViskCompiler;

using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal sealed class ViskStack
{
    public const int BlockSize = 8;
    public const sbyte PosStackAlign = -16;

    private const int MinValue = BlockSize * 1;

    public readonly int MaxStackSize;

    private int _offset = MinValue;

    public ViskStack(int maxStackSize)
    {
        MaxStackSize = maxStackSize;
    }

    public AssemblerMemoryOperand GetNext()
    {
        var assemblerMemoryOperand = __[rbp - _offset];
        _offset += BlockSize;
        if (_offset > MaxStackSize)
            ThrowHelper.ThrowInvalidOperationException("Stack overflowed");
        return assemblerMemoryOperand;
    }

    public AssemblerMemoryOperand GetPrevious()
    {
        _offset -= BlockSize;
        if (_offset < MinValue)
            ThrowHelper.ThrowInvalidOperationException("No value to return");
        return __[rbp - _offset];
    }

    public bool IsEmpty() => _offset <= MinValue;

    public AssemblerMemoryOperand BackValue()
    {
        if (_offset - BlockSize < MinValue)
            ThrowHelper.ThrowInvalidOperationException("No value to return");
        return __[rbp - (_offset - BlockSize)];
    }
}