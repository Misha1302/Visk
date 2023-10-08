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
    GotoIfFalse,
    Dup,
    Cmp,
    Drop,
    SetArg,
    LogicNeg,
    GotoIfTrue
}