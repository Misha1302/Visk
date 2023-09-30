namespace ViskCompiler;

using Iced.Intel;

internal class ViskDataManager
{
    private readonly Assembler _assembler;
    private readonly List<(Label, long)> _data = new();

    public readonly ViskRegister Register = new();
    public readonly Dictionary<string, Label> Labels = new();
    public readonly Stack<AssemblerMemoryOperand> Stack = new();

    public ViskDataManager(Assembler assembler)
    {
        _assembler = assembler;
    }

    public IReadOnlyList<(Label, long)> Data => _data;

    public Label DefineI64(long l)
    {
        var ind = _data.FindIndex(x => x.Item2 == l);
        if (ind != -1)
            return _data[ind].Item1;

        var label = _assembler.CreateLabel();
        _data.Add((label, l));
        return label;
    }
}