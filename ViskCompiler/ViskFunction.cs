namespace ViskCompiler;

public sealed class ViskFunction
{
    public readonly string Name;
    public readonly List<ViskInstruction> Instructions = new();
    private readonly Dictionary<string, int> _locals = new();

    public IReadOnlyDictionary<string, int> Locals = null!;

    public ViskFunction(string name)
    {
        Name = name;
    }

    public int StackAlloc { get; private set; }

    public List<ViskInstruction> TotalInstructions => Prepare(Instructions);

    private int MaxStackSize
    {
        get
        {
            var size = 0;
            var max = 0;

            foreach (var i in Instructions)
            {
                var c = ViskInstruction.InstructionCharacteristics[i.InstructionKind];
                if (i.InstructionKind != ViskInstructionKind.CallForeign)
                    size += c.output - c.args;
                else
                    size += (int)i.Arguments[3] - (int)i.Arguments[1];
                max = Math.Max(max, size - (ViskRegister.Registers.Length - 1));
            }

            return max;
        }
    }

    private List<ViskInstruction> Prepare(List<ViskInstruction> instructions)
    {
        StackAlloc = Locals.Count * 8 + MaxStackSize * 8;
        Locals = GetLocals();
        var instr = ViskInstruction.Prolog(StackAlloc);

        if (instructions[0].InstructionKind != ViskInstructionKind.Prolog)
            instructions.Insert(0, instr);
        else instructions[0] = instr;

        return instructions;
    }

    private IReadOnlyDictionary<string, int> GetLocals()
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