namespace ViskCompiler;

// ReSharper disable InconsistentNaming
public enum ViskInstructionKind : long
{
    DoubleInstruction = PushConstD | SetLocalD | LoadLocalD | SetArgD | AddD | SubD | MulD | DivD,


    PushConst = 1L << 0,
    Add = 1L << 1,
    Sub = 1L << 2,
    Ret = 1L << 3,
    CallForeign = 1L << 4,
    IMul = 1L << 5,
    SetLabel = 1L << 6,
    Goto = 1L << 7,
    Prolog = 1L << 8,
    SetLocal = 1L << 9,
    LoadLocal = 1L << 10,
    Nop = 1L << 11,
    Call = 1L << 12,
    IfTrue = 1L << 13,
    Dup = 1L << 14,
    Equals = 1L << 15,
    Drop = 1L << 16,
    SetArg = 1L << 17,
    SetArgD = 1L << 18,
    LogicNeg = 1L << 19,
    IfFalse = 1L << 20,
    IDiv = 1L << 21,
    NotEquals = 1L << 22,
    PushConstD = 1L << 23,
    AddD = 1L << 24,
    SubD = 1L << 25,
    MulD = 1L << 26,
    DivD = 1L << 27,
    SetLocalD = 1L << 28,
    LoadLocalD = 1L << 29,
    DupD = 1L << 30,
    DropD = 1L << 31,
    RetD = 1L << 32,
    LessThan = 1L << 33,
    LessThanOrEquals = 1L << 34,
    GreaterThan = 1L << 35,
    GreaterThanOrEquals = 1L << 36
}