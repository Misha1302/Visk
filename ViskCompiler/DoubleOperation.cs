namespace ViskCompiler;

internal enum DoubleOperation : byte
{
    AreEqual = 0,
    LessThan = 1,
    LessThanOrEquals = 2,
    GreaterThanOrEquals = 0x0D,
    GreaterThan = 0x0E,
    AreNotEquals = 0x14
}