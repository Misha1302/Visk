namespace ViskCompiler;

using System.Diagnostics.Contracts;
using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal sealed class ViskFunctionStackManager
{
    private readonly ViskDataManager _dataManager;

    internal ViskFunctionStackManager(ViskDataManager dataManager)
    {
        _dataManager = dataManager;
    }

    [Pure]
    public AssemblerMemoryOperand GetMemoryToSaveReg(int index) =>
        __[rbp - _dataManager.CurrentFuncMaxStackSize - (index + 1) * ViskStack.BlockSize];

    [Pure]
    public AssemblerMemoryOperand GetStackMemory(int offset) => __[rbp - offset];

    [Pure]
    public AssemblerMemoryOperand GetMemoryLocal(int offset) => __[
        rbp - _dataManager.CurrentFuncMaxStackSize - ViskRegister.Registers.Length * ViskStack.BlockSize - offset
    ];

    [Pure]
    public AssemblerMemoryOperand GetMemoryArg(int nextArgIndex) => __[rbp + nextArgIndex];
}