namespace Visk;

using System.Runtime.CompilerServices;

public static class Helper
{
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void PrintLongs(
        long l1, long l2, long l3, long l4, long l5, long l6
        )
    {
        Console.WriteLine(l1 * 1.5f);
        Console.WriteLine(l2);
        Console.WriteLine(l3);
        Console.WriteLine(l4);
        Console.WriteLine(l5);
        Console.WriteLine(l6);
    }

    public static long GetFour() => (long)(23.4f / GetFive());
    private static long GetFive() => 5;
}