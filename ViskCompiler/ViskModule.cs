namespace ViskCompiler;

public sealed class ViskModule
{
    private readonly List<ViskFunction> _functions = new();
    public readonly string? MainFuncName;

    public ViskModule(string? mainFuncNameName)
    {
        MainFuncName = mainFuncNameName;
    }

    public IReadOnlyList<ViskFunction> Functions => _functions;

    public ViskFunction AddFunction(string name)
    {
        var f = new ViskFunction(name);
        _functions.Add(f);
        return f;
    }
}