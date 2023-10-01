namespace ViskCompiler;

// ReSharper disable InconsistentNaming
public enum ViskInstructionKind
{
    PushConst,
    Add,
    Ret,
    CallForeign,
    IMul,
    SetLabel,
    Goto,
    Prolog,
    SetLocal,
    LoadLocal,
    Nop,
    Call
}