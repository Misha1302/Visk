namespace ViskCompiler;

// ReSharper disable InconsistentNaming
public enum ViskInstructionKind
{
    PushConst,
    Add,
    Sub,
    Ret,
    CallForeign,
    IMul,
    SetLabel,
    Goto,
    Prolog,
    SetLocal,
    LoadLocal,
    Nop,
    Call,
    GotoIfNotEquals,
    Dup,
    Cmp,
    Drop,
    SetArg,
    LogicNeg
}