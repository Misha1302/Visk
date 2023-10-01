namespace ViskCompiler;

public sealed class ViskModule
{
    private readonly List<ViskFunction> _functions = new();
    public readonly string MainFuncName;

    public ViskModule(string? mainFuncNameName)
    {
        MainFuncName = mainFuncNameName ?? "main";
    }

    public IReadOnlyList<ViskFunction> Functions => _functions;

    public ViskFunction AddFunction(string name, int argsCount, bool returns)
    {
        var f = new ViskFunction(name, argsCount, returns);
        _functions.Add(f);
        return f;
    }
}