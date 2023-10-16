namespace ViskCompiler;

public sealed class ViskLocalsOptimizer
{
    public void Optimize(ViskInstruction[] viskInstructions)
    {
        var locals = new List<ViskInstruction>();

        foreach (var instruction in viskInstructions)
            switch (instruction.InstructionKind)
            {
                case ViskInstructionKind.SetLocal:
                case ViskInstructionKind.SetArg:
                case ViskInstructionKind.SetLocalD:
                case ViskInstructionKind.SetArgD:
                    locals.Add(instruction);
                    break;
#warning
                case ViskInstructionKind.LoadRef:
                    // create variable in the stack or on the heap
                    var name = instruction.Arguments[0].As<string>();
                    foreach (var loc in locals.Where(x => x.Arguments[0].As<string>() == name))
                        loc.Arguments[1] = ViskLocType.Ref;
                    break;
            }
    }
}