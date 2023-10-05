namespace ViskCompiler;

using System.Diagnostics.Contracts;

public static class Extensions
{
    [Pure]
    public static T As<T>(this object? o) => (T)(o ?? ThrowHelper.ThrowInvalidOperationException<T>());

    [Pure]
    public static long AsI64(this object? o)
    {
        return o switch
        {
            long l => l,
            int i => i,
            short s => s,
            byte b => b,
            _ => ThrowHelper.ThrowInvalidOperationException<int>("Obj is not integer")
        };
    }

    [Pure]
    public static int AsI32(this object? o) => (int)AsI64(o);
}