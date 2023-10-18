namespace ViskCompiler;

public static class ViskBuildInCharacteristics
{
    public static readonly IReadOnlyDictionary<ViskBuildIn, (int input, int output)> BuildIns =
        new Dictionary<ViskBuildIn, (int input, int output)>
        {
            [ViskBuildIn.Alloc] = (1, 1),
            [ViskBuildIn.Free] = (1, 1)
        };
}