namespace ViskCompiler;

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static ViskThrowHelper;

public class ViskBuildInFunctions
{
    public readonly nint AllocPtr;
    public readonly nint FreePtr;

    public ViskBuildInFunctions()
    {
        AllocPtr = GetMethodPtr(nameof(Alloc));
        FreePtr = GetMethodPtr(nameof(Free));
    }

    private nint GetMethodPtr(string name)
    {
        var info = GetType().GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)
                   ?? ThrowInvalidOperationException<MethodInfo>();

        RuntimeHelpers.PrepareMethod(info.MethodHandle);
        return info.MethodHandle.GetFunctionPointer();
    }

    private static long Alloc(long bytes) => Marshal.AllocHGlobal((int)bytes);
    private static void Free(long ptr) => Marshal.FreeHGlobal((nint)ptr);

    public static unsafe string ViskPtrToStr(long ptr)
    {
        var len = *(long*)ptr;
        Span<char> span = new char[len];
        for (var i = 0; i < len; i++)
            span[i] = *(char*)(ptr + sizeof(long) + i * sizeof(char));
        return new string(span);
    }
}