namespace ViskCompiler;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

internal static class ViskThrowHelper
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    public static T ThrowInvalidOperationException<T>(string s = "") => throw new InvalidOperationException(s);

    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    public static object ThrowInvalidOperationException(string s = "") => ThrowInvalidOperationException<object>(s);
}