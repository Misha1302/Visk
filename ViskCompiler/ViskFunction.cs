namespace ViskCompiler;

public sealed class ViskFunction
{
    public readonly string Name;
    public readonly int ArgsCount;
    public readonly bool Returns;

    public readonly List<ViskInstruction> RawInstructions = new();
    private Dictionary<string, int> _locals = new();


    public ViskFunction(string name, int argsCount, bool returns)
    {
        ArgsCount = argsCount;
        Returns = returns;
        Name = name;
    }

    public IReadOnlyDictionary<string, int> Locals
    {
        get => _locals;
        set => _locals = (Dictionary<string, int>)value;
    }

    public List<ViskInstruction> TotalInstructions => Prepare(RawInstructions);

    public int MaxStackSize { get; private set; }

    private List<ViskInstruction> Prepare(List<ViskInstruction> instructions)
    {
        CheckAllArgsNotNull();
        Locals = GetLocals();
        MaxStackSize = GetMaxStackSize();
        var instr = ViskInstruction.Prolog();

        if (instructions[0].InstructionKind != ViskInstructionKind.Prolog)
            instructions.Insert(0, instr);
        else instructions[0] = instr;

        return instructions;
    }

    private void CheckAllArgsNotNull()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (RawInstructions.Any(i => i.Arguments.Any(x => x == null)))
            ThrowHelper.ThrowInvalidOperationException("Some arg is null");
    }

    private int GetMaxStackSize()
    {
        var size = 0;
        var max = 0;

        foreach (var i in RawInstructions)
        {
            if (i.InstructionKind != ViskInstructionKind.CallForeign)
            {
                var c = ViskInstruction.InstructionCharacteristics[i.InstructionKind];
                size += c.output - c.args;
            }
            else if (i.InstructionKind == ViskInstructionKind.Call)
            {
                var f = (ViskFunction)i.Arguments[0];
                size += f.Returns ? 1 : 0 - f.ArgsCount;
            }
            else
            {
                size += ((bool)i.Arguments[3] ? 1 : 0) - (int)i.Arguments[1];
            }

            max = Math.Max(max, size - ViskRegister.Registers.Length);
        }

        return max;
    }

    private IReadOnlyDictionary<string, int> GetLocals()
    {
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var x in RawInstructions)
        {
            if (x.InstructionKind != ViskInstructionKind.SetLocal)
                continue;

            // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
            var args = x.Arguments ?? ThrowHelper.ThrowInvalidOperationException<List<object>>();
            _locals.TryAdd((string)args[0], (_locals.Count + 1) * 8);
        }

        return _locals;
    }
}