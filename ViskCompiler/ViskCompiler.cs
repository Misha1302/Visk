namespace ViskCompiler;

using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

public sealed class ViskCompiler
{
    private const int StackAlignConst = 16;

    private readonly List<ViskInstruction> _instructions;
    private readonly Assembler _assembler = new(64);
    private readonly List<(Label, long)> _data = new();
    private int _stackOffset = 8;

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
        var arg2 = inst.Arguments?.Length > 2 ? inst.Arguments[2] : null;

        switch (inst.InstructionKind)
        {
            case ViskInstructionKind.PushConst:
                var data = arg0 switch
                {
                    long l => l,
                    int i => i,
                    double d => BitConverter.DoubleToInt64Bits(d),
                    _ => throw new InvalidOperationException()
                };
                if (data > int.MaxValue)
                {
                    _assembler.mov(rax, __[DefineI64(data)]);
                    PushRax();
                }
                else
                {
                    Push((int)data);
                }

                break;
            case ViskInstructionKind.Add:
                Pop(rbx);
                Pop(rax);
                _assembler.add(rax, rbx);
                PushRax();
                break;
            case ViskInstructionKind.IMul:
                Pop(rbx);
                Pop(rax);
                _assembler.imul(rax, rbx);
                PushRax();
                break;
            case ViskInstructionKind.Ret:
                _assembler.ret();
                break;
            case ViskInstructionKind.Drop:
                _assembler.pop(rax);
                break;
            case ViskInstructionKind.CSharpRet:
                _assembler.pop(rax);
                _assembler.ret();
                break;
            case ViskInstructionKind.CallForeign:
                var ptr = (nint)(arg0 ?? throw new InvalidOperationException());
                var argsCount = (int)(arg1 ?? throw new InvalidOperationException());
                var returnsType = (ViskType)(arg2 ?? throw new InvalidOperationException());

                SetRegisters(argsCount);
                AlignStack();
                _assembler.call((ulong)ptr);
                UnAlignStack();

                if (ViskType.R64.HasFlag(returnsType))
                {
                    if (returnsType == ViskType.Float64)
                        _assembler.movq(rax, xmm0);

                    PushRax();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(inst), inst.InstructionKind.ToString());
        }
    }

    private void Push(int data)
    {
        _assembler.push(data);
        _stackOffset += 8;
    }

    private void AlignStack()
    {
        var delta = _stackOffset % StackAlignConst;
        if (delta != 0)
            _assembler.sub(rsp, delta);
    }

    private void UnAlignStack()
    {
        var delta = _stackOffset % StackAlignConst;
        if (delta != 0)
            _assembler.add(rsp, delta);
    }

    private void Pop(AssemblerRegister64 r)
    {
        _assembler.pop(r);
        _stackOffset -= 8;
    }

    private void PushRax()
    {
        _assembler.push(rax);
        _stackOffset += 8;
    }

    private void SetRegisters(int argsCount)
    {
        //TODO:
    }

    private Label DefineI64(long l)
    {
        var label = _assembler.CreateLabel();
        _data.Add((label, l));
        return label;
    }
}