namespace ViskCompiler;

using Iced.Intel;

public static class Extensions
{
    public static Label GetOrAdd(this Dictionary<string, Label> dictionary, Assembler assembler, string s)
    {
        Label label;
        if (!dictionary.TryAdd(s, label = assembler.CreateLabel(s)))
            label = dictionary[s];

        return label;
    }

    public static int GetMax<T>(this IEnumerable<T> collection, Func<T, int> p)
    {
        var max = 0;
        foreach (var c in collection) 
            max = Math.Max(p(c), max);
        return max;
    }
}