namespace ViskCompiler;

public sealed class ViskImage
{
    private readonly ViskModule _viskModule = null!;

    public ViskImage(ViskModule viskModule)
    {
        ViskModule = viskModule;
    }

    private ViskModule ViskModule
    {
        get => _viskModule;
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        init => _viskModule = value ?? ThrowHelper.ThrowInvalidOperationException<ViskModule>();
    }

    public ViskX64AsmExecutor Compile() => new ViskCompiler(ViskModule).Compile();
}