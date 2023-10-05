namespace ViskCompiler;

using System.Diagnostics.Contracts;
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
        var directivesCount = 0;

        for (var index = 0; index < _asm.Instructions.Count; index++)
        {
            var instr = _asm.Instructions[index];

            output.List.Clear();

            formatter.Format(instr, output);

            for (var i = 0; i < output.List.Count; i++)
            {
                var (text, kind) = output.List[i];

                if (kind != FormatterTextKind.Directive && i == 0)
                    sb.Append($"{index.ToString().PadLeft(maxDigits)}: ");

                if (kind == FormatterTextKind.Directive)
                {
                    if (directivesCount == 0)
                        sb.AppendLine("-------------------------\n");

                    directivesCount++;
                    sb.Append(directivesCount + ". ");
                }

                sb.Append(ToPrintableStr(text));
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    [Pure]
    private static string ToPrintableStr(string text) =>
        text switch
        {
            "," => ", ",
            "nop" => "\n",
            _ => text
        };

    private sealed class FormatterOutputImpl : FormatterOutput
    {
        public readonly List<(string text, FormatterTextKind kind)> List = new();
        public override void Write(string text, FormatterTextKind kind) => List.Add((text, kind));
    }
}