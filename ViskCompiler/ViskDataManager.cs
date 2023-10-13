namespace ViskCompiler;

using System.Diagnostics.Contracts;
using Iced.Intel;

internal sealed class ViskDataManager
{
    private readonly List<(Label, long)> _data = new();
    private readonly Dictionary<string, Label> _labels = new();

    public readonly ViskModule Module;
    public readonly Assembler Assembler;
    public readonly ViskRegister Register = new();
    public readonly ViskFunctionStackManager FuncStackManager;
    public readonly ViskArgsManager ViskArgsManager;
    public readonly IViskDebugInfo DebugInfo;

    public ViskDataManager(Assembler? assembler, ViskModule? module, ViskSettings viskSettings)
    {
        Assembler = assembler ?? ViskThrowHelper.ThrowInvalidOperationException<Assembler>();
        Module = module ?? ViskThrowHelper.ThrowInvalidOperationException<ViskModule>();
        FuncStackManager = new ViskFunctionStackManager(this);
        ViskArgsManager = new ViskArgsManager(this);

        if (viskSettings.ViskCompilationMode == ViskCompilationMode.Debug)
            DebugInfo = new ViskDebugInfo();
        else DebugInfo = new ViskDebugInfoPlug();
    }

    public ViskStack Stack { get; private set; } = null!;

    public IReadOnlyList<(Label, long)> Data => _data;

    public int CurrentFuncMaxStackSize { get; private set; }

    public ViskLocals CurrentFuncLocals { get; private set; } = null!;

    public void NewFunc(int stackSize, IReadOnlyList<ViskLocal> locals)
    {
        CurrentFuncMaxStackSize = stackSize * 8;
        Stack = new ViskStack(this);
        Register.Reset();
        ViskArgsManager.Reset();

        CurrentFuncLocals = new ViskLocals(locals, FuncStackManager);
    }

    [Pure]
    public Label DefineI64(long l)
    {
        var ind = _data.FindIndex(x => x.Item2 == l);
        if (ind != -1)
            return _data[ind].Item1;

        var label = Assembler.CreateLabel();
        _data.Add((label, l));
        return label;
    }

    [Pure]
    public Label GetLabel(string name)
    {
        if (_labels.TryGetValue(name, out var label))
            return label;

        _labels.Add(name, label = Assembler.CreateLabel(name));
        return label;
    }
}