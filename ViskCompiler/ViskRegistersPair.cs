namespace ViskCompiler;

internal sealed class ViskRegistersPair
{
    private readonly AssemblerRegister64 _x64;
    private readonly AssemblerRegister16 _x16;
    private readonly AssemblerRegisterXMM _xmm;

    public ViskRegistersPair(AssemblerRegister16 x16, AssemblerRegister64 x64, AssemblerRegisterXMM xmm)
    {
        _x64 = x64;
        _xmm = xmm;
        _x16 = x16;
    }

    public AssemblerRegister64 X64 => _x64 == default
        ? ViskThrowHelper.ThrowInvalidOperationException<AssemblerRegister64>("Invalid register")
        : _x64;

    public AssemblerRegister16 X16 => _x16 == default
        ? ViskThrowHelper.ThrowInvalidOperationException<AssemblerRegister16>("Invalid register")
        : _x16;

    public AssemblerRegisterXMM Xmm => _xmm == default
        ? ViskThrowHelper.ThrowInvalidOperationException<AssemblerRegisterXMM>("Invalid register")
        : _xmm;
}