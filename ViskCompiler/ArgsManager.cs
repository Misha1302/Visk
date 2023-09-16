namespace ViskCompiler;

using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

internal static class ArgsManager
{
    private static readonly AssemblerRegister64[] _assemblerRegisters = { rcx, rdx, r8, r9 };

    public static void MoveArgs(int argsCount, ViskRegister fromReg, Assembler assembler, out bool stackAligned)
    {
        stackAligned = false;

        var reg = new ViskRegister(fromReg.CurValue);

        /*
        Maybe this table is true?
        
        number. size - total_size - fill_offset
        
        1. 8 - 32 - 0

        2. 16 - 48 - 8
        3. 24 - 48 - 0

        4. 32 - 64 - 8
        5. 40 - 64 - 0

        6. 48 - 80 - 8
        7. 56 - 80 - 0 
        */

        if (argsCount >= _assemblerRegisters.Length)
        {
            stackAligned = true;
            var size = (argsCount - _assemblerRegisters.Length) * 8;
            var maxSize = 32 + (size & 16);

            assembler.sub(rsp, maxSize);

            var i = maxSize - (size + 8) % 16;
            var k = argsCount - _assemblerRegisters.Length;
            while (i >= 0 && k > 0)
            {
                assembler.mov(__[rsp + i], reg.Previous());
                i -= 8;
                k--;
            }
        }

        var max = Math.Min(argsCount, _assemblerRegisters.Length);
        for (var j = max - 1; j >= 0; j--)
            assembler.mov(_assemblerRegisters[j], reg.Previous());
    }
}