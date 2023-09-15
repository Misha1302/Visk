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
}