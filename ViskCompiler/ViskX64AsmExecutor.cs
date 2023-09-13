﻿namespace ViskCompiler;

using System.Runtime.InteropServices;
using Iced.Intel;

public sealed partial class ViskX64AsmExecutor
{
    public delegate long StdAsmDelegate();

    public enum AllocationType
    {
        MemCommit = 0x00001000
    }

    public enum ProtectionType
    {
        PageExecuteReadwrite = 0b1000000
    }

    private readonly Assembler _asm;

    public ViskX64AsmExecutor(Assembler asm)
    {
        _asm = asm;
    }

    [LibraryImport("kernel32.dll")]
    private static partial nint VirtualAlloc(nint lpAddress, int dwSize, uint flAllocationType, uint flProtect);

    public StdAsmDelegate GetDelegate()
    {
        var stream = new MemoryStream();
        var streamCodeWriter = new StreamCodeWriter(stream);
        _asm.Assemble(streamCodeWriter, 0);

        var mem = VirtualAlloc(IntPtr.Zero, (int)stream.Length, (uint)AllocationType.MemCommit,
            (uint)ProtectionType.PageExecuteReadwrite);
        Marshal.Copy(stream.ToArray(), 0, mem, (int)stream.Length);

        return Marshal.GetDelegateForFunctionPointer<StdAsmDelegate>(mem);
    }

    public override string ToString() => new ViskDecompiler(_asm).ToString();
}