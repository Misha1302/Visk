namespace ViskCompiler;

public sealed class ViskFunction
{
    public readonly ViskFunctionInfo Info;

    public readonly List<ViskInstruction> RawInstructions = new();
    private List<string> _locals = new();


    public ViskFunction(string name, int argsCount, Type returnType, bool isMain = false)
    {
        Info = new ViskFunctionInfo(name, argsCount, returnType, isMain);
    }

    public IReadOnlyList<string> Locals
    {
        get => _locals;
        private set => _locals = (List<string>)value;
    }

    public List<ViskInstruction> TotalInstructions => Prepare(RawInstructions);

    public int MaxStackSize { get; private set; }

    private List<ViskInstruction> Prepare(List<ViskInstruction> instructions)
    {
        if (instructions.Count != 0 && instructions[0].InstructionKind == ViskInstructionKind.Nop)
            instructions.RemoveAt(0);
        if (instructions.Count == 0)
            instructions.Add(ViskInstruction.Nop());

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
            ViskThrowHelper.ThrowInvalidOperationException("Some arg is null");
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
                    Max(f.Info.ArgsCount, f.Info.ReturnType != typeof(void) ? 1 : 0);
                    break;
                default:
                    var c = ViskInstruction.InstructionCharacteristics[i.InstructionKind];
                    Max(c.args, c.output);
                    break;
            }

            max = Math.Max(max, size - ViskRegister.PublicRegisters.Length);
        }

        return max;

        void Max(int args, int output)
        {
            size += output;
            max = Math.Max(max + args, size - ViskRegister.PublicRegisters.Length);
            size -= args;
        }
    }

    private List<string> GetLocals()
    {
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var x in RawInstructions)
        {
            if (x.InstructionKind is not ViskInstructionKind.SetLocal and not ViskInstructionKind.SetArg)
                continue;

            if (!_locals.Contains(x.Arguments[0].As<string>()))
                _locals.Add((string)x.Arguments[0]);
        }

        return _locals;
    }
}