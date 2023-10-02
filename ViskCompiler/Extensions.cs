namespace ViskCompiler;

public static class Extensions
{
    public static T As<T>(this object? o) => (T)(o ?? ThrowHelper.ThrowInvalidOperationException<T>());

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
}