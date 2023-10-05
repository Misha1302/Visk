namespace ViskCompiler;

using System.Diagnostics.Contracts;
using Iced.Intel;

internal sealed class ViskStack
{
    public const int BlockSize = 8;
    public const sbyte PosStackAlign = -16;
    private const int MinValue = BlockSize * 1;

    private readonly ViskDataManager _dataManager;
    public readonly int MaxStackSize;

    private int _offset = MinValue;

    public ViskStack(ViskDataManager dataManager)
    {
        _dataManager = dataManager;
        MaxStackSize = dataManager.CurrentFuncMaxStackSize;
    }

    public AssemblerMemoryOperand GetNext()
    {
        var assemblerMemoryOperand = _dataManager.FuncStackManager.GetStackMemory(_offset);
        _offset += BlockSize;
        if (_offset > MaxStackSize)
            ViskThrowHelper.ThrowInvalidOperationException("Stack overflowed");
        return assemblerMemoryOperand;
    }

    public AssemblerMemoryOperand GetPrevious()
    {
        _offset -= BlockSize;
        if (_offset < MinValue)
            ViskThrowHelper.ThrowInvalidOperationException("No value to return");
        return _dataManager.FuncStackManager.GetStackMemory(_offset);
    }

    [Pure]
    public bool IsEmpty() => _offset <= MinValue;

    public AssemblerMemoryOperand BackValue()
    {
        if (_offset - BlockSize < MinValue)
            ViskThrowHelper.ThrowInvalidOperationException("No value to return");
        return _dataManager.FuncStackManager.GetStackMemory(_offset - BlockSize);
    }
}