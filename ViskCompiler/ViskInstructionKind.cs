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
    Equals,
    Drop,
    SetArg,
    LogicNeg,
    GotoIfTrue,
    IDiv,
    NotEquals
}