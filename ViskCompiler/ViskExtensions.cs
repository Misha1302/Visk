namespace ViskCompiler;

using System.Diagnostics.Contracts;

public static class ViskExtensions
{
    [Pure]
    public static T As<T>(this object? o) => (T)(o ?? ViskThrowHelper.ThrowInvalidOperationException<T>());

    [Pure]
    public static long AsI64(this object? o)
    {
        return o switch
        {
            long l => l,
            int i => i,
            short s => s,
            byte b => b,
            _ => ViskThrowHelper.ThrowInvalidOperationException<int>("Obj is not integer")
        };
    }

    [Pure]
    public static int AsI32(this object? o) => (int)AsI64(o);

    public static void SaveFile(this string? text, string fileName) => File.WriteAllText(fileName, text);

    public static async void SaveFileAsync(this string? text, string fileName) =>
        await File.WriteAllTextAsync(fileName, text);
}