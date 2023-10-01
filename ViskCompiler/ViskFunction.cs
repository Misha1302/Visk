namespace ViskCompiler;

public sealed class ViskFunction
{
    public readonly string Name;
    public readonly int ArgsCount;
    public readonly bool Returns;

    public readonly List<ViskInstruction> Instructions = new();
    private readonly Dictionary<string, int> _locals = new();

    public IReadOnlyDictionary<string, int> Locals = null!;


    public ViskFunction(string name, int argsCount, bool returns)
    {
        ArgsCount = argsCount;
        Returns = returns;
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
                if (i.InstructionKind != ViskInstructionKind.CallForeign)
                {
                    var c = ViskInstruction.InstructionCharacteristics[i.InstructionKind];
                    size += c.output - c.args;
                }
                else if (i.InstructionKind == ViskInstructionKind.Call)
                {
                    var f = (ViskFunction)i.Arguments[0];
                    size += f.Returns ? 1 : 0  - f.ArgsCount;
                }
                else
                {
                    size += ((bool)i.Arguments[3] ? 1 : 0) - (int)i.Arguments[1];
                }

                max = Math.Max(max, size - ViskRegister.Registers.Length);
            }

            return max;
        }
    }

    private List<ViskInstruction> Prepare(List<ViskInstruction> instructions)
    {
        Locals = GetLocals();
        StackAlloc = Locals.Count * 8 + MaxStackSize * 8;
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