namespace ViskCompiler;

using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

public sealed class ViskCompiler
{
    private const byte StackAlignConst = unchecked((byte)-16);

    private readonly List<ViskInstruction> _instructions;
    private readonly Assembler _assembler = new(64);
    private readonly ViskRegister _register = new();
    private readonly List<(Label, long)> _data = new();
    private readonly Dictionary<string, Label> _labels = new();

    public ViskCompiler(List<ViskInstruction> instructions)
    {
        _instructions = instructions;
    }

    public ViskX64AsmExecutor Compile()
    {
        _instructions.ForEach(CompileInstruction);


        _data.ForEach(x =>
        {
            _assembler.Label(ref x.Item1);
            _assembler.dq(x.Item2);
        });

        return new ViskX64AsmExecutor(_assembler);
    }

    private void CompileInstruction(ViskInstruction inst)
    {
        var arg0 = inst.Arguments?.Length > 0 ? inst.Arguments[0] : null;
        var arg1 = inst.Arguments?.Length > 1 ? inst.Arguments[1] : null;

        Label label;
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
                _assembler.add(_register.Previous(), _register.Previous());
                break;
            case ViskInstructionKind.Ret:
                _assembler.mov(rax, _register.Previous());
                _register.Reset();

                _assembler.mov(rsp, rbp);
                _assembler.pop(rbp);
                _assembler.ret();
                break;
            case ViskInstructionKind.CallForeign:
                ArgsManager.MoveArgsToRegisters(
                    (int)(arg1 ?? throw new InvalidOperationException()), _register, _assembler
                );

                AlignStack();
                _assembler.call((ulong)(nint)(arg0 ?? throw new InvalidOperationException()));
                if (_register.CurValue != rax)
                    _assembler.mov(_register.Next(), rax);
                break;
            case ViskInstructionKind.IMul:
                _assembler.imul(_register.Previous(), _register.Previous());
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
                _assembler.sub(rsp, (int)(arg0 ?? throw new InvalidOperationException()));
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