namespace ViskCompiler;

using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal sealed class ViskCompiler
{
    private const byte StackAlignConst = unchecked((byte)-16);

    private readonly ViskModule _module;
    private readonly Assembler _assembler = new(64);
    private readonly ViskRegister _register = new();
    private readonly List<(Label, long)> _data = new();
    private readonly Dictionary<string, Label> _labels = new();

    public ViskCompiler(ViskModule module)
    {
        _module = module;
    }

    public ViskX64AsmExecutor Compile()
    {
        ((List<ViskFunction>)_module.Functions).ForEach(CompileInstructions);


        _data.ForEach(x =>
        {
            _assembler.Label(ref x.Item1);
            _assembler.dq(x.Item2);
        });

        return new ViskX64AsmExecutor(_assembler);
    }

    private void CompileInstructions(ViskFunction function)
    {
        foreach (var instr in function.TotalInstructions)
            CompileInstruction(instr, function);
    }

    private void CompileInstruction(ViskInstruction inst, ViskFunction function)
    {
        var arg0 = inst.Arguments.Count > 0 ? inst.Arguments[0] : null;
        var arg1 = inst.Arguments.Count > 1 ? inst.Arguments[1] : null;
        var arg2 = inst.Arguments.Count > 2 ? inst.Arguments[2] : null;
        var arg3 = inst.Arguments.Count > 3 ? inst.Arguments[3] : null;

        Label label;
        int localOffset;
        AssemblerRegister64 prev;
        switch (inst.InstructionKind)
        {
            case ViskInstructionKind.PushConst:
                var integer = arg0 switch
                {
                    long l => l,
                    int i => i,
                    float f => BitConverter.SingleToInt32Bits(f),
                    double d => BitConverter.DoubleToInt64Bits(d),
                    _ => throw new ArgumentOutOfRangeException()
                };

                _assembler.mov(_register.Next(), __[DefineI64(integer)]);
                break;
            case ViskInstructionKind.Add:
                prev = _register.Previous();
                _assembler.add(_register.BackValue(), prev);
                break;
            case ViskInstructionKind.Ret:
                _assembler.mov(rax, _register.Previous());
                
                _register.Reset();

                _assembler.mov(rsp, rbp);
                _assembler.pop(rbp);
                _assembler.ret();
                break;
            case ViskInstructionKind.CallForeign:
                var argsCount = (int)(arg1 ?? throw new InvalidOperationException());
                var dataInStack = ((List<string>)(arg2 ?? throw new InvalidOperationException()))
                    .Select(x => function.Locals[x]).ToList();
                
                

                foreach (var r in ViskRegister.Registers) 
                    _assembler.push(r);
                _assembler.mov(r13, rsp);
                
                ArgsManager.MoveArgs(
                    argsCount, _register, _assembler, dataInStack, out var stackAligned
                );
                _register.Sub(argsCount - dataInStack.Count);

                if (!stackAligned)
                    AlignStack();

                _assembler.call((ulong)(nint)(arg0 ?? throw new InvalidOperationException()));
                _assembler.mov(rsp, r13);
                
                foreach (var r in ViskRegister.Registers.Reverse()) 
                    _assembler.pop(r);

                if ((bool)(arg3 ?? throw new InvalidOperationException()))
                    _assembler.mov(_register.Next(), rax);
                break;
            case ViskInstructionKind.IMul:
                prev = _register.Previous();
                _assembler.imul(_register.BackValue(), prev);
                break;
            case ViskInstructionKind.SetLabel:
                label = _labels.GetOrAdd(_assembler, (string)(arg0 ?? throw new InvalidOperationException()));
                _assembler.Label(ref label);
                break;
            case ViskInstructionKind.Goto:
                label = _labels.GetOrAdd(_assembler, (string)(arg0 ?? throw new InvalidOperationException()));
                _assembler.jmp(label);
                break;
            case ViskInstructionKind.Prolog:
                _assembler.push(rbp);
                _assembler.mov(rbp, rsp);

                var allocCount = (int)(arg0 ?? throw new InvalidOperationException());
                if (allocCount != 0)
                    _assembler.sub(rsp, allocCount + allocCount % 16);
                break;
            case ViskInstructionKind.SetLocal:
                localOffset = function.Locals[(string)(arg0 ?? throw new InvalidOperationException())];
                _assembler.mov(__[rbp - localOffset], _register.Previous());
                break;
            case ViskInstructionKind.LoadLocal:
                localOffset = function.Locals[(string)(arg0 ?? throw new InvalidOperationException())];
                _assembler.mov(_register.Next(), __[rbp - localOffset]);
                break;
            case ViskInstructionKind.Nop:
                _assembler.nop();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(inst), inst.InstructionKind.ToString());
        }
    }

    private void AlignStack()
    {
        _assembler.and(sp, StackAlignConst);
    }

    private Label DefineI64(long l)
    {
        var label = _assembler.CreateLabel();
        _data.Add((label, l));
        return label;
    }
}