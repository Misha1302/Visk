namespace ViskCompiler;

// ReSharper disable InconsistentNaming
public enum ViskInstructionKind
{
    PushConst,
    Drop,
    Add,
    Ret,
    CSharpRet,
    CallForeign,
    IMul
}