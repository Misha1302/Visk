namespace ViskCompiler;

public class ViskImage
{
    private readonly ViskModule _viskModule = null!;

    public ViskImage(ViskModule viskModule)
    {
        ViskModule = viskModule;
    }

    private ViskModule ViskModule
    {
        get => _viskModule;
        init => _viskModule = value ?? throw new InvalidOperationException();
    }

    public ViskX64AsmExecutor Compile() => new ViskCompiler(ViskModule).Compile();
}