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

    private readonly Stack<Type> _stack = new();
    private int _offset = MinValue;

    public ViskStack(ViskDataManager dataManager)
    {
        _dataManager = dataManager;
        MaxStackSize = dataManager.CurrentFuncMaxStackSize;
    }

    public IReadOnlyList<Type> Stack => _stack.ToArray();

    public AssemblerMemoryOperand GetNext(Type type)
    {
        var assemblerMemoryOperand = _dataManager.FuncStackManager.GetStackMemory(_offset);
        _offset += BlockSize;
        _stack.Push(type);
        if (_offset > MaxStackSize)
            ViskThrowHelper.ThrowInvalidOperationException("Stack overflowed");
        return assemblerMemoryOperand;
    }

    public AssemblerMemoryOperand GetPrevious(out Type type)
    {
        _offset -= BlockSize;
        type = _stack.Pop();
        if (_offset < MinValue)
            ViskThrowHelper.ThrowInvalidOperationException("No value to return");
        return _dataManager.FuncStackManager.GetStackMemory(_offset);
    }

    public AssemblerMemoryOperand GetPrevious() => GetPrevious(out _);

    [Pure]
    public bool IsEmpty() => _offset <= MinValue;

    public AssemblerMemoryOperand BackValue(out Type type)
    {
        type = _stack.Pop();
        if (_offset - BlockSize < MinValue)
            ViskThrowHelper.ThrowInvalidOperationException("No value to return");
        return _dataManager.FuncStackManager.GetStackMemory(_offset - BlockSize);
    }

    public AssemblerMemoryOperand BackValue() => BackValue(out _);
}