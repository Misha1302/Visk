namespace ViskCompiler;

// ReSharper disable InconsistentNaming
public enum ViskInstructionKind : long
{
    PushConst = 1 << 0,
    Add = 1 << 1,
    Sub = 1 << 2,
    Ret = 1 << 3,
    CallForeign = 1 << 4,
    IMul = 1 << 5,
    SetLabel = 1 << 6,
    Goto = 1 << 7,
    Prolog = 1 << 8,
    SetLocal = 1 << 9,
    LoadLocal = 1 << 10,
    Nop = 1 << 11,
    Call = 1 << 12,
    GotoIfFalse = 1 << 13,
    Dup = 1 << 14,
    Equals = 1 << 15,
    Drop = 1 << 16,
    SetArg = 1 << 17,
    SetArgD = 1 << 18,
    LogicNeg = 1 << 19,
    GotoIfTrue = 1 << 20,
    IDiv = 1 << 21,
    NotEquals = 1 << 22,
    PushConstD = 1 << 23,
    AddD = 1 << 24,
    SubD = 1 << 25,
    MulD = 1 << 26,
    DivD = 1 << 27,
    SetLocalD = 1 << 28,
    LoadLocalD = 1 << 29,
    DoubleInstruction = PushConstD | SetLocalD | LoadLocalD | SetArgD | AddD | SubD | MulD | DivD
}