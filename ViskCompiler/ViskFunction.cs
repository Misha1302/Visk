namespace ViskCompiler;

public sealed class ViskFunction
{
    public readonly string Name;
    public readonly int ArgsCount;
    public readonly Type ReturnType;

    public readonly List<ViskInstruction> RawInstructions = new();
    private Dictionary<string, int> _locals = new();


    public ViskFunction(string name, int argsCount, Type returnType)
    {
        ArgsCount = argsCount;
        ReturnType = returnType;
        Name = name;
    }

    public IReadOnlyDictionary<string, int> Locals
    {
        get => _locals;
        private set => _locals = (Dictionary<string, int>)value;
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
            switch (i.InstructionKind)
            {
                case ViskInstructionKind.CallForeign:
                    Max((int)i.Arguments[1], i.Arguments[2].As<Type>() != typeof(void) ? 1 : 0);
                    break;
                case ViskInstructionKind.Call:
                    var f = i.Arguments[0].As<ViskFunction>();
                    Max(f.ArgsCount, f.ReturnType != typeof(void) ? 1 : 0);
                    break;
                default:
                    var c = ViskInstruction.InstructionCharacteristics[i.InstructionKind];
                    Max(c.args, c.output);
                    break;
            }

            max = Math.Max(max, size - ViskRegister.Registers.Length);
        }

        return max;

        void Max(int args, int output)
        {
            size += output;
            max = Math.Max(max + args, size - ViskRegister.Registers.Length);
            size -= args;
        }
    }

    private IReadOnlyDictionary<string, int> GetLocals()
    {
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var x in RawInstructions)
        {
            if (x.InstructionKind is not ViskInstructionKind.SetLocal and not ViskInstructionKind.SetArg)
                continue;

            // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
            var args = x.Arguments ?? ThrowHelper.ThrowInvalidOperationException<List<object>>();
            _locals.TryAdd((string)args[0], (_locals.Count + 1) * 8);
        }

        return _locals;
    }
}