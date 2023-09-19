namespace ViskCompiler;

public static class Preprocessor
{
    private static readonly Dictionary<ViskInstructionKind, int> _opArgs = new()
    {
        [ViskInstructionKind.PushConst] = -1,
        [ViskInstructionKind.Add] = -1,
        [ViskInstructionKind.Ret] = -1,
        [ViskInstructionKind.CallForeign] = 10_000,
        [ViskInstructionKind.IMul] = -1,
        [ViskInstructionKind.SetLabel] = 0,
        [ViskInstructionKind.Goto] = 0,
        [ViskInstructionKind.Prolog] = 0,
        [ViskInstructionKind.SetLocal] = 1,
        [ViskInstructionKind.LoadLocal] = -1,
        [ViskInstructionKind.Nop] = 0
    };

    public static void Preprocess(ViskModule module)
    {
        //TODO: 'infinity' args or 'infinity' values in stack

        foreach (var f in module.Functions) 
            Preprocess(f);
    }

    private static void Preprocess(ViskFunction f)
    {
        var pushesCount = 0;

        List<string> lst = new();
        for (var index = 0; index < f.Instructions.Count; index++)
        {
            var i = f.Instructions[index];

            if (i.InstructionKind == ViskInstructionKind.CallForeign)
            {
                pushesCount -= (int)(i.Arguments[1] ?? throw new InvalidOperationException());
                i.Arguments.Add(lst);
                lst = new List<string>();
            }
            else
            {
                var delta = _opArgs[i.InstructionKind];
                pushesCount -= delta;

                if (pushesCount < ViskRegister.Registers.Length)
                    continue;


                switch (delta)
                {
                    case < 0:
                        var name = Guid.NewGuid().ToString();
                        var instr = ViskInstruction.SetLocal(name);
                        f.Instructions.Insert(index + 1, instr);
                        index++;
                        lst.Add(name);
                        break;
                    case > 1:
                        i.Arguments.RemoveAt(i.Arguments.Count - 1);
                        break;
                }
            }
        }
    }
}