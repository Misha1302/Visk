namespace ViskCompiler;

public sealed class ViskFunction
{
    public readonly ViskFunctionInfo Info;

    private readonly List<ViskInstruction> _rawInstructions = new();
    private List<ViskLocal> _locals = new();


    public ViskFunction(string name, List<Type> parameters, Type returnType, bool isMain = false)
    {
        Info = new ViskFunctionInfo(name, parameters, returnType, isMain);
    }

    public IReadOnlyList<ViskLocal> Locals
    {
        get => _locals;
        private set => _locals = (List<ViskLocal>)value;
    }

    public List<ViskInstruction> TotalInstructions => Prepare(_rawInstructions);

    public int MaxStackSize { get; private set; }

    private List<ViskInstruction> Prepare(List<ViskInstruction> instructions)
    {
        if (instructions.Count != 0 && instructions[0].InstructionKind == ViskInstructionKind.Nop)
            instructions.RemoveAt(0);
        if (instructions.Count == 0)
            instructions.Add(ViskInstruction.Nop());

        CheckAllArgsNotNull();
        Locals = GetLocals();
        MaxStackSize = GetMaxStackSize(_rawInstructions);
        var instr = ViskInstruction.Prolog();

        if (instructions[0].InstructionKind != ViskInstructionKind.Prolog)
            instructions.Insert(0, instr);
        else instructions[0] = instr;

        return instructions;
    }

    private void CheckAllArgsNotNull()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (_rawInstructions.Any(i => i.Arguments.Any(x => x == null)))
            ViskThrowHelper.ThrowInvalidOperationException("Some arg is null");
    }

    private int GetMaxStackSize(List<ViskInstruction> viskInstructions)
    {
        var size = 0;
        var max = 0;

        foreach (var i in viskInstructions)
        {
            switch (i.InstructionKind)
            {
                case ViskInstructionKind.CallForeign:
                    Max(i.Arguments[1].As<List<Type>>().Count, i.Arguments[2].As<Type>() != ViskConsts.None ? 1 : 0);
                    break;
                case ViskInstructionKind.Call:
                    var f = i.Arguments[0].As<ViskFunction>();
                    Max(f.Info.Params.Count, f.Info.ReturnType != ViskConsts.None ? 1 : 0);
                    break;
                case ViskInstructionKind.CallBuildIn:
                    var io = ViskBuildInCharacteristics.BuildIns[i.Arguments[0].As<ViskBuildIn>()];
                    Max(io.input, io.output);
                    break;
                case ViskInstructionKind.IfTrue:
                    Max(1, GetMaxStackSize(i.Arguments[0].As<List<ViskInstruction>>()));
                    break;
                case ViskInstructionKind.IfFalse:
                    Max(1, GetMaxStackSize(i.Arguments[0].As<List<ViskInstruction>>()));
                    break;
                default:
                    var c = ViskInstruction.InstructionCharacteristics[i.InstructionKind];
                    Max(c.args, c.output);
                    break;
            }

            max = Math.Max(max, size - ViskRegister.PublicRegisters.Length);
        }

        if (max >= 100_000)
            ViskThrowHelper.ThrowInvalidOperationException("Max stack is too big. Maybe there is error here?");

        return max;

        void Max(int args, int output)
        {
            size += output;
            max = Math.Max(max + args, size - ViskRegister.PublicRegisters.Length);
            size -= args;
        }
    }

    private List<ViskLocal> GetLocals()
    {
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var x in _rawInstructions)
        {
            if (x.InstructionKind
                is not ViskInstructionKind.SetLocal
                and not ViskInstructionKind.SetArg
                and not ViskInstructionKind.SetArgD
                and not ViskInstructionKind.SetLocalD)
                continue;

            var type = ViskInstructionKind.DoubleInstruction.HasFlag(x.InstructionKind)
                ? ViskConsts.F64
                : ViskConsts.I64;

            if (_locals.All(y => y.Name != x.Arguments[0].As<string>()))
                _locals.Add(new ViskLocal(type, x.Arguments[0].As<string>(), x.Arguments[1].As<ViskLocType>()));
        }

        return _locals;
    }

    public override string ToString() => Info.ToString();

    public void AddInstructions(params ViskInstruction[] viskInstructions)
    {
        new ViskLocalsOptimizer().Optimize(viskInstructions);
        _rawInstructions.AddRange(viskInstructions);
    }

    public void AddInstructions(List<ViskInstruction> viskInstructions)
    {
        var instructions = viskInstructions.ToArray();
        AddInstructions(instructions);
    }
}