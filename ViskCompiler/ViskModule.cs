namespace ViskCompiler;

public sealed class ViskModule
{
    private readonly List<ViskFunction> _functions = new();
    public readonly string MainFuncName;

    public ViskModule(string? mainFuncName)
    {
        MainFuncName = mainFuncName ?? "main";
    }

    public IReadOnlyList<ViskFunction> Functions => _functions;

    public ViskFunction AddFunction(string name, List<Type> parameters, Type returnType)
    {
        var f = new ViskFunction(name, parameters, returnType, name == MainFuncName);
        _functions.Add(f);
        return f;
    }
}