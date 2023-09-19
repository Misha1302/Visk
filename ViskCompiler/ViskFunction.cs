namespace ViskCompiler;

public sealed class ViskFunction
{
    public readonly string Name;
    public readonly List<ViskInstruction> Instructions = new();
    private readonly Dictionary<string, int> _locals = new();

    public ViskFunction(string name)
    {
        Name = name;
    }

    public IReadOnlyDictionary<string, int> Locals
    {
        get
        {
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var x in Instructions)
            {
                if (x.InstructionKind != ViskInstructionKind.SetLocal)
                    continue;

                var args = x.Arguments ?? throw new InvalidOperationException();
                _locals.TryAdd((string)args[0], (_locals.Count + 1) * 8);
            }

            return _locals;
        }
    }

    public List<ViskInstruction> TotalInstructions => Prepare(Instructions);

    private List<ViskInstruction> Prepare(List<ViskInstruction> instructions)
    {
        var instr = ViskInstruction.Prolog(Locals.Count * 8);

        if (instructions[0].InstructionKind != ViskInstructionKind.Prolog)
            instructions.Insert(0, instr);
        else instructions[0] = instr;

        return instructions;
    }

    public void AddRange(List<ViskInstruction> viskInstructions)
    {
        Instructions.AddRange(viskInstructions);
    }
}