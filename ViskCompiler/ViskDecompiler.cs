namespace ViskCompiler;

using System.Text;
using Iced.Intel;

public sealed class ViskDecompiler
{
    private readonly Assembler _asm;

    public ViskDecompiler(Assembler asm)
    {
        _asm = asm;
    }

    public override string ToString()
    {
        StringBuilder sb = new();

        var maxDigits = (int)Math.Log(_asm.Instructions.Count);

        var formatter = new IntelFormatter();
        var output = new FormatterOutputImpl();
        var firstDefine = true;

        for (var index = 0; index < _asm.Instructions.Count; index++)
        {
            var instr = _asm.Instructions[index];

            output.List.Clear();

            formatter.Format(instr, output);

            Console.ResetColor();

            for (var i = 0; i < output.List.Count; i++)
            {
                var (text, kind) = output.List[i];

                if (kind != FormatterTextKind.Directive && i == 0)
                        Console.Write($"{index.ToString().PadLeft(maxDigits)}: ");

                if (kind == FormatterTextKind.Directive && firstDefine)
                {
                    firstDefine = false;
                    Console.WriteLine("-------------------------\n");
                }

                Console.ForegroundColor = GetColor(kind);
                Console.Write(ToPrintableStr(text));
            }

            Console.WriteLine();
        }

        Console.ResetColor();

        return sb.ToString();
    }

    private static string ToPrintableStr(string text)
    {
        return text switch
        {
            "," => ", ",
            "nop" => "\nnop",
            _ => text
        };
    }

    private static ConsoleColor GetColor(FormatterTextKind kind)
    {
        switch (kind)
        {
            case FormatterTextKind.Directive:
            case FormatterTextKind.Keyword:
                return ConsoleColor.Yellow;
            case FormatterTextKind.Prefix:
            case FormatterTextKind.Mnemonic:
                return ConsoleColor.Red;
            case FormatterTextKind.Register:
                return ConsoleColor.Magenta;
            case FormatterTextKind.Number:
                return ConsoleColor.Green;
            default:
                return ConsoleColor.White;
        }
    }

    private sealed class FormatterOutputImpl : FormatterOutput
    {
        public readonly List<(string text, FormatterTextKind kind)> List = new();
        public override void Write(string text, FormatterTextKind kind) => List.Add((text, kind));
    }
}