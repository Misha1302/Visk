namespace ViskCompiler;

using System.Diagnostics.Contracts;
using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal sealed class ViskFunctionStackManager
{
    private readonly ViskDataManager _dataManager;
    private readonly int _publicRegistersLength;
    private readonly int _totalRegistersLength;

    internal ViskFunctionStackManager(ViskDataManager dataManager)
    {
        _publicRegistersLength = ViskRegister.PublicRegisters.Length * ViskStack.BlockSize;
        var publicRegistersDLength = ViskRegister.PublicRegistersD.Length * ViskStack.BlockSize;
        _totalRegistersLength = _publicRegistersLength + publicRegistersDLength;
        _dataManager = dataManager;
    }

    [Pure]
    public AssemblerMemoryOperand GetMemoryToSaveRegX64(int index) =>
        __[rbp - _dataManager.CurrentFuncMaxStackSize - (index + 1) * ViskStack.BlockSize];

    [Pure]
    public AssemblerMemoryOperand GetMemoryToSaveRegD(int index) =>
        __[rbp - _dataManager.CurrentFuncMaxStackSize - _publicRegistersLength - (index + 2) * ViskStack.BlockSize];

    [Pure]
    public AssemblerMemoryOperand GetStackMemory(int offset) => __[rbp - offset];

    [Pure]
    public AssemblerMemoryOperand GetMemoryLocal(int offset) =>
        __[rbp - _dataManager.CurrentFuncMaxStackSize - _totalRegistersLength - offset];

    [Pure]
    public AssemblerMemoryOperand GetMemoryArg(int nextArgIndex) => __[rbp + nextArgIndex];
}