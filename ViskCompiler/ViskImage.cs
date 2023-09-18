namespace ViskCompiler;

public class ViskImage
{
    private readonly ViskModule? _viskModule;
    private bool _preprocessed;

    public ViskImage(ViskModule viskModule)
    {
        ViskModule = viskModule;
    }

    private ViskModule ViskModule
    {
        get
        {
            if (!_preprocessed)
            {
                _preprocessed = true;
                Preprocess();
            }

            return _viskModule!;
        }
        init => _viskModule = value ?? throw new InvalidOperationException();
    }

    private void Preprocess()
    {
        Preprocessor.Preprocess(ViskModule);
    }

    public ViskX64AsmExecutor Compile() => new ViskCompiler(ViskModule).Compile();
}