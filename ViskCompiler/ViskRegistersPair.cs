namespace ViskCompiler;

internal sealed class ViskRegistersPair
{
    private readonly AssemblerRegister64 _x64;
    private readonly AssemblerRegisterXMM _xmm;

    public ViskRegistersPair(AssemblerRegister64 x64, AssemblerRegisterXMM xmm)
    {
        _x64 = x64;
        _xmm = xmm;
    }

    public AssemblerRegister64 X64 => _x64 == default
        ? ViskThrowHelper.ThrowInvalidOperationException<AssemblerRegister64>("Invalid register")
        : _x64;

    public AssemblerRegisterXMM Xmm => _xmm == default
        ? ViskThrowHelper.ThrowInvalidOperationException<AssemblerRegisterXMM>("Invalid register")
        : _xmm;
}