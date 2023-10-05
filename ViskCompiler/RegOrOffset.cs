namespace ViskCompiler;

using Iced.Intel;

internal sealed class RegOrOffset
{
    private readonly ViskStack _dataInStack;
    private readonly ViskRegister _register;

    public RegOrOffset(ViskStack dataInStack, ViskRegister register)
    {
        _dataInStack = dataInStack;
        _register = register;
    }

    public bool GetRegisterOrOffset(out AssemblerRegister64? register64, out AssemblerMemoryOperand? offset) =>
        StackAtFirst(out register64, out offset);

    private bool StackAtFirst(out AssemblerRegister64? register64, out AssemblerMemoryOperand? offset)
    {
        if (!_dataInStack.IsEmpty())
        {
            offset = _dataInStack.GetPrevious();
            register64 = null;
            return false;
        }

        offset = null;
        register64 = _register.Previous();
        return true;
    }
}