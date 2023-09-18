namespace ViskCompiler;

public sealed class ViskFunction
{
    public readonly string Name;
    private readonly List<ViskInstruction> _instructions = new();
    private readonly Dictionary<string, int> _locals = new();

    public ViskFunction(string name)
    {
        Name = name;
    }

    public IReadOnlyDictionary<string, int> Locals => _locals;

    public IReadOnlyList<ViskInstruction> Instructions => Prepare(_instructions);

    private List<ViskInstruction> Prepare(List<ViskInstruction> instructions)
    {
        var instr = ViskInstruction.Prolog(_locals.Count * 8);

        if (instructions[0].InstructionKind != ViskInstructionKind.Prolog)
            instructions.Insert(0, instr);
        else instructions[0] = instr;

        return instructions;
    }

    public void AddRange(List<ViskInstruction> viskInstructions)
    {
        viskInstructions.ForEach(x =>
        {
            if (x.InstructionKind != ViskInstructionKind.SetLocal)
                return;

            var args = x.Arguments ?? throw new InvalidOperationException();
            _locals.TryAdd((string)args[0], _locals.Count * 8);
        });

        _instructions.AddRange(viskInstructions);
    }
}