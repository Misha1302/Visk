namespace ViskCompiler;

using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal class ViskDataManager
{
    private readonly List<(Label, long)> _data = new();
    private readonly Dictionary<string, Label> _labels = new();

    public readonly ViskModule Module;
    public readonly Assembler Assembler;
    public readonly Dictionary<string, Label> Functions = new();
    public readonly ViskRegister Register = new();

    public ViskDataManager(Assembler? assembler, ViskModule? module)
    {
        Assembler = assembler ?? ThrowHelper.ThrowInvalidOperationException<Assembler>();
        Module = module ?? ThrowHelper.ThrowInvalidOperationException<ViskModule>();
    }

    public ViskStack Stack { get; private set; } = null!;

    public IReadOnlyList<(Label, long)> Data => _data;

    public int CurrentFuncMaxStackSize { get; private set; }

    public IReadOnlyDictionary<string, AssemblerMemoryOperand> CurrentFuncLocals { get; private set; } = null!;
    public int CurrentFuncLocalsSize => CurrentFuncLocals.Count;

    public void NewFunc(int stackSize, IReadOnlyDictionary<string, int> locals)
    {
        Stack = new ViskStack(stackSize * 8);
        CurrentFuncMaxStackSize = stackSize * 8;
        Register.Reset();
        
        CurrentFuncLocals = locals
            .Select(x => (
                    key: x.Key,
                    value: __[
                        rbp - CurrentFuncMaxStackSize - ViskRegister.Registers.Length * ViskStack.BlockSize - x.Value
                    ]
                )
            )
            .ToDictionary(tuple => tuple.key, tuple => tuple.value);
    }

    public Label DefineI64(long l)
    {
        var ind = _data.FindIndex(x => x.Item2 == l);
        if (ind != -1)
            return _data[ind].Item1;

        var label = Assembler.CreateLabel();
        _data.Add((label, l));
        return label;
    }

    public Label GetLabel(string name)
    {
        if (_labels.TryGetValue(name, out var label))
            return label;

        _labels.Add(name, label = Assembler.CreateLabel(name));
        return label;
    }
}