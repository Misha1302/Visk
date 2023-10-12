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
    SetArgD,
    LogicNeg,
    GotoIfTrue,
    IDiv,
    NotEquals,
    PushConstD,
    AddD,
    SubD,
    MulD,
    DivD,
    SetLocalD,
    LoadLocalD,
    DoubleInstruction = PushConstD | SetLocalD | LoadLocalD | SetArgD | AddD | SubD | MulD | DivD
}