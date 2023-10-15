namespace ViskCompiler;

using Iced.Intel;

internal sealed class ViskRegOrOffset
{
    private readonly ViskStack _dataInStack;
    private readonly ViskRegister _register;

    public ViskRegOrOffset(ViskStack dataInStack, ViskRegister register)
    {
        _dataInStack = dataInStack;
        _register = register;
    }

    public void GetRegisterOrOffset(Type curType,
        out AssemblerRegister64? register64, out AssemblerRegisterXMM? registerXmm, out AssemblerMemoryOperand? offset)
    {
        register64 = null;
        registerXmm = null;
        offset = null;

        if (!_dataInStack.IsEmpty())
        {
            offset = _dataInStack.GetPrevious();
            return;
        }

        if (curType == ViskConsts.I64)
            register64 = _register.Rx64.Previous();
        else if (curType == ViskConsts.F64)
            registerXmm = _register.Rd.Previous();
        else ViskThrowHelper.ThrowInvalidOperationException($"Unknown type {curType}");
    }
}