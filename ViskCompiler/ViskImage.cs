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
        init => _viskModule = value ?? ViskThrowHelper.ThrowInvalidOperationException<ViskModule>();
    }

    public ViskX64AsmExecutor Compile(ViskSettings settings) => new ViskCompiler(ViskModule, settings).Compile();
}