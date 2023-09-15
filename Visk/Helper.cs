namespace Visk;

public static class Helper
{
    public static void PrintLongMultipliedByHalf(long l) => Console.WriteLine(l * 0.5f);

    public static long GetFour() => (long)(23.4f / GetFive());
    private static long GetFive() => 5;
}