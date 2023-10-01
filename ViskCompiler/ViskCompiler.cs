namespace ViskCompiler;

using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal sealed class ViskCompiler
{
    private const byte StackAlignConst = unchecked((byte)-32);

    private readonly ViskModule _module;
    private readonly Assembler _assembler = new(64);
    private readonly ViskDataManager _dataManager;
    private readonly Dictionary<string, Label> _functions = new();

    public ViskCompiler(ViskModule module)
    {
        _module = module;
        _dataManager = new ViskDataManager(_assembler);
    }

    public ViskX64AsmExecutor Compile()
    {
        _functions.Add(_module.MainFuncName, _assembler.CreateLabel(_module.MainFuncName));
        _assembler.jmp(_functions[_module.MainFuncName]);

        foreach (var f in _module.Functions)
            CompileInstructions(f);

        foreach (var x in _dataManager.Data)
        {
            var copy = x;
            _assembler.Label(ref copy.Item1);
            _assembler.dq(copy.Item2);
        }

        return new ViskX64AsmExecutor(_assembler);
    }

    private void CompileInstructions(ViskFunction function)
    {
        if (function.Instructions.Last(x => x.InstructionKind != ViskInstructionKind.Nop).InstructionKind !=
            ViskInstructionKind.Ret)
            throw new InvalidOperationException();

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
        Label funcLabel;
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

                if (_dataManager.Register.CanGetNext)
                {
                    _assembler.mov(_dataManager.Register.Next(), __[_dataManager.DefineI64(integer)]);
                }
                else
                {
                    _assembler.mov(rax, __[_dataManager.DefineI64(integer)]);
                    var offset = (_dataManager.Stack.Count + 1) * 8;
                    _dataManager.Stack.Push(
                        function.Locals.Count != 0
                            ? __[rbp - (function.Locals.Max(x => x.Value) + offset)]
                            : __[rbp - offset]
                    );
                    _assembler.mov(_dataManager.Stack.Peek(), rax);
                }

                break;
            case ViskInstructionKind.Add:
                if (_dataManager.Stack.Count != 0)
                    _assembler.add(_dataManager.Register.BackValue(), _dataManager.Stack.Pop());
                else
                    _assembler.add(_dataManager.Register.BackValue(), _dataManager.Register.Previous());

                break;
            case ViskInstructionKind.Ret:
                if (_dataManager.Register.CanGetPrevious)
                    _assembler.mov(rax, _dataManager.Register.Previous());

                _dataManager.Register.Reset();

                _assembler.mov(rsp, rbp);
                _assembler.pop(rbp);
                _assembler.ret();
                break;
            case ViskInstructionKind.CallForeign:
                PrepareCall((int)(arg1 ?? throw new InvalidOperationException()));
                _assembler.call((ulong)(nint)(arg0 ?? throw new InvalidOperationException()));
                AfterCall((bool)(arg3 ?? throw new InvalidOperationException()));
                break;
            case ViskInstructionKind.Call:
                var func = (ViskFunction)(arg0 ?? throw new InvalidOperationException());

                PrepareCall(func.ArgsCount);

                if (!_functions.TryGetValue(func.Name, out funcLabel))
                {
                    funcLabel = _assembler.CreateLabel(funcLabel.Name);
                    _functions.Add(func.Name, funcLabel);
                }

                _assembler.call(funcLabel);

                AfterCall(function.Returns);
                break;
            case ViskInstructionKind.IMul:
                _assembler.imul(_dataManager.Register.BackValue(), _dataManager.Register.Previous());
                break;
            case ViskInstructionKind.SetLabel:
                label = _dataManager.Labels.GetOrAdd(_assembler,
                    (string)(arg0 ?? throw new InvalidOperationException()));
                _assembler.Label(ref label);
                break;
            case ViskInstructionKind.Goto:
                label = _dataManager.Labels.GetOrAdd(_assembler,
                    (string)(arg0 ?? throw new InvalidOperationException()));
                _assembler.jmp(label);
                break;
            case ViskInstructionKind.Prolog:
                if (!_functions.TryGetValue(function.Name, out funcLabel))
                {
                    funcLabel = _assembler.CreateLabel(funcLabel.Name);
                    _functions.Add(function.Name, funcLabel);
                }

                _assembler.Label(ref funcLabel);

                _assembler.push(rbp);
                _assembler.mov(rbp, rsp);

                var allocCount = (int)(arg0 ?? throw new InvalidOperationException());
                if (allocCount != 0)
                    _assembler.sub(rsp, allocCount + allocCount % 16);
                break;
            case ViskInstructionKind.SetLocal:
                localOffset = function.Locals[(string)(arg0 ?? throw new InvalidOperationException())];
                _assembler.mov(__[rbp - localOffset], _dataManager.Register.Previous());
                break;
            case ViskInstructionKind.LoadLocal:
                localOffset = function.Locals[(string)(arg0 ?? throw new InvalidOperationException())];
                _assembler.mov(_dataManager.Register.Next(), __[rbp - localOffset]);
                break;
            case ViskInstructionKind.Nop:
                _assembler.nop();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(inst), inst.InstructionKind.ToString());
        }
    }

    private void AfterCall(bool returns)
    {
        _assembler.mov(rax, __[rsp + 8]);
        _assembler.mov(rsp, rax);

        _assembler.pop(r12);
        _assembler.pop(r11);
        _assembler.pop(r10);
        _assembler.pop(rbx);

        if (returns)
            _assembler.mov(_dataManager.Register.Next(), rax);
    }

    private void PrepareCall(int argsCount)
    {
        ArgsManager.MoveArgs(
            argsCount, _dataManager.Register, _assembler, _dataManager.Stack
        );
        _dataManager.Register.Sub(argsCount - _dataManager.Stack.Count);

        _assembler.push(rbx);
        _assembler.push(r10);
        _assembler.push(r11);
        _assembler.push(r12);

        _assembler.mov(rax, rsp);
        AlignStack();
        _assembler.sub(sp, 16);
        _assembler.mov(__[rsp + 8], rax);
    }

    private void AlignStack()
    {
        _assembler.and(sp, StackAlignConst);
    }
}