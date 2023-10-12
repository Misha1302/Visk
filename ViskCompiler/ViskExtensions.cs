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
    public static double AsF64(this object? o)
    {
        return o switch
        {
            Half s => (double)s,
            float l => l,
            double i => i,
            decimal b => (double)b,
            _ => ViskThrowHelper.ThrowInvalidOperationException<int>("Obj is not double")
        };
    }

    [Pure]
    public static int AsI32(this object? o) => (int)AsI64(o);

    public static void SaveFile(this string? text, string fileName) => File.WriteAllText(fileName, text);

    public static async void SaveFileAsync(this string? text, string fileName) =>
        await File.WriteAllTextAsync(fileName, text);

    public static int BinarySearch<T>(this IList<T> list, Func<T, int> comparer)
    {
        var min = 0;
        var max = list.Count - 1;

        while (min <= max)
        {
            var mid = (min + max) / 2;
            var comparison = comparer(list[mid]);
            switch (comparison)
            {
                case 0:
                    return mid;
                case < 0:
                    min = mid + 1;
                    break;
                default:
                    max = mid - 1;
                    break;
            }
        }

        return ~min;
    }
}