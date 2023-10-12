namespace Visk;

using System.Runtime.CompilerServices;

public static class Helper
{
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void PrintLongs(
        long l1, long l2, long l3, long l4, long l5, long l6, long l7, long l8, long l9, long l10
    )
    {
        Console.WriteLine(l1); // * 1.5f
        Console.WriteLine(l2);
        Console.WriteLine(l3);
        Console.WriteLine(l4);
        Console.WriteLine(l5);
        Console.WriteLine(l6);
        Console.WriteLine(l7);
        Console.WriteLine(l8);
        Console.WriteLine(l9);
        Console.WriteLine(l10);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void PrintLong(long l1)
    {
        Console.WriteLine(l1);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void PrintDouble(double d, long l)
    {
        Console.WriteLine(d);
        Console.WriteLine(l);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void PrintSmt()
    {
        Console.WriteLine("Smt");
    }

    public static long GetFour() => (long)(23.4f / GetFive());
    private static long GetFive() => 5;
}