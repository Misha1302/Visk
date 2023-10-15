namespace ViskCompiler;

using System.Collections.Immutable;

internal sealed class ViskRegisterInternal<T>
{
    private readonly ImmutableArray<T> _publicRegisters;

    public ViskRegisterInternal(ImmutableArray<T> publicRegisters)
    {
        _publicRegisters = publicRegisters;
    }


    public int CurIndex { get; private set; }

    public bool CanGetPrevious => CurIndex - 1 >= 0;
    public bool CanGetNext => CurIndex < _publicRegisters.Length;

    public int CurIndexD { get; private set; }

    public T Next() =>
        _publicRegisters[
            CurIndex >= _publicRegisters.Length
                ? ViskThrowHelper.ThrowInvalidOperationException<int>("All registers are used")
                : CurIndex++
        ];

    public T Previous() =>
        _publicRegisters[
            CurIndex - 1 < 0
                ? ViskThrowHelper.ThrowInvalidOperationException<int>("No register to return")
                : --CurIndex
        ];

    public void Reset() => CurIndex = CurIndexD = 0;

    public T BackValue() => _publicRegisters[
        CurIndex - 1 < 0
            ? ViskThrowHelper.ThrowInvalidOperationException<int>("No register to return")
            : CurIndex - 1
    ];

    public T Current() => _publicRegisters[CurIndex];

    public ViskRegisterInternal<T> Copy() => (ViskRegisterInternal<T>)MemberwiseClone();
}