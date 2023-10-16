namespace TestProject;

using System.Reflection;
using ViskCompiler;

public sealed class Tests
{
    private static readonly MethodInfo _smallMethodInfo =
        typeof(Tests).GetMethod(nameof(SmallMethod), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo _voidMethodInfo =
        typeof(Tests).GetMethod(nameof(VoidMethod), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo _doubleToLongMethodInfo =
        typeof(Tests).GetMethod(nameof(DoubleToLong), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo _sum15DoublesMethodInfo =
        typeof(Tests).GetMethod(nameof(Sum15Doubles), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo _bigMethodInfo =
        typeof(Tests).GetMethod(nameof(BigMethod), BindingFlags.NonPublic | BindingFlags.Static)!;

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test00()
    {
        ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.Ret()
            }
        );

        Assert.Pass();
    }

    [Test]
    public void Test01()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConst(5),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(5));
    }

    [Test]
    public void Test02()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConst(5),
                ViskInstruction.PushConst(-3),
                ViskInstruction.Add(),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(2));
    }

    [Test]
    public void Test03()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConst(5),
                ViskInstruction.PushConst(-3),
                ViskInstruction.IMul(),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(-15));
    }

    [Test]
    public void Test04()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConst(5),
                ViskInstruction.PushConst(-3),
                ViskInstruction.IDiv(),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(-1));
    }

    [Test]
    public void Test05()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConst(5),
                ViskInstruction.PushConst(5),
                ViskInstruction.PushConst(5),
                ViskInstruction.PushConst(5),
                ViskInstruction.PushConst(5),
                ViskInstruction.PushConst(5),
                ViskInstruction.PushConst(5),
                ViskInstruction.PushConst(5),
                ViskInstruction.PushConst(5),
                ViskInstruction.PushConst(5),

                ViskInstruction.Add(),
                ViskInstruction.Add(),
                ViskInstruction.Add(),
                ViskInstruction.Add(),
                ViskInstruction.Add(),
                ViskInstruction.Add(),
                ViskInstruction.Add(),
                ViskInstruction.Add(),
                ViskInstruction.Add(),

                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(50));
    }

    [Test]
    public void Test06()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConst(-2),
                ViskInstruction.SetLocal("i"),

                ViskInstruction.LoadLocal("i"),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(-2));
    }

    [Test]
    public void Test07()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConst(0),
                ViskInstruction.SetLocal("i"),


                ViskInstruction.SetLabel("m"),

                ViskInstruction.PushConst(1),
                ViskInstruction.LoadLocal("i"),
                ViskInstruction.Add(),
                ViskInstruction.SetLocal("i"),

                ViskInstruction.LoadLocal("i"),
                ViskInstruction.PushConst(10),
                ViskInstruction.NotEquals(),
                ViskInstruction.IfTrue(ViskInstruction.Goto("m")),

                ViskInstruction.LoadLocal("i"),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(10));
    }

    [Test]
    public void Test08()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConst(0),
                ViskInstruction.SetLocal("i"),

                ViskInstruction.PushConst(2),
                ViskInstruction.SetLocal("j"),


                ViskInstruction.SetLabel("m"),

                ViskInstruction.LoadLocal("j"),
                ViskInstruction.LoadLocal("i"),
                ViskInstruction.Add(),
                ViskInstruction.SetLocal("i"),

                ViskInstruction.LoadLocal("i"),
                ViskInstruction.PushConst(100),
                ViskInstruction.NotEquals(),
                ViskInstruction.IfTrue(ViskInstruction.Goto("m")),


                ViskInstruction.LoadLocal("i"),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(100));
    }

    [Test]
    public void Test09()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                // if you try to put it lower, you will catch division by zero exception
                ViskInstruction.PushConst(2_000_000_000),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),

                ViskInstruction.IDiv(),
                ViskInstruction.IDiv(),
                ViskInstruction.IDiv(),
                ViskInstruction.IDiv(),
                ViskInstruction.IDiv(),
                ViskInstruction.IDiv(),
                ViskInstruction.IDiv(),
                ViskInstruction.IDiv(),
                ViskInstruction.IDiv(),

                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(1000000000));
    }

    [Test]
    public void Test10()
    {
        var result = ExecuteFunctions(
            module =>
            {
                var mf = module.AddFunction("main", new List<Type>(0), ViskConsts.I64);
                var of = module.AddFunction("other", new List<Type>(0), ViskConsts.I64);

                mf.AddInstructions(
                    ViskInstruction.Call(of),
                    ViskInstruction.Call(of),
                    ViskInstruction.Add(),
                    ViskInstruction.Ret()
                );

                of.AddInstructions(
                    ViskInstruction.PushConst(23),
                    ViskInstruction.PushConst(2),
                    ViskInstruction.Add(),
                    ViskInstruction.Ret()
                );
            }
        );

        Assert.That(result, Is.EqualTo(50));
    }

    [Test]
    public void Test11()
    {
        var result = ExecuteFunctions(
            module =>
            {
                var mf = module.AddFunction("main", new List<Type>(0), ViskConsts.I64);
                var of = module.AddFunction("other", new List<Type> { ViskConsts.I64 }, ViskConsts.I64);

                mf.AddInstructions(
                    ViskInstruction.PushConst(5),
                    ViskInstruction.Call(of),
                    ViskInstruction.Call(of),
                    ViskInstruction.Ret()
                );

                of.AddInstructions(
                    ViskInstruction.SetArg("arg"),
                    ViskInstruction.LoadLocal("arg"),
                    ViskInstruction.PushConst(2),
                    ViskInstruction.IMul(),
                    ViskInstruction.Ret()
                );
            }
        );

        Assert.That(result, Is.EqualTo(20));
    }

    [Test]
    public void Test12()
    {
        var result = ExecuteFunctions(
            module =>
            {
                var mf = module.AddFunction("main", new List<Type>(0), ViskConsts.I64);
                var of = module.AddFunction("other",
                    Enumerable.Repeat(ViskConsts.I64, 10).ToList(),
                    ViskConsts.I64);

                mf.AddInstructions(
                    ViskInstruction.PushConst(1),
                    ViskInstruction.PushConst(2),
                    ViskInstruction.PushConst(3),
                    ViskInstruction.PushConst(4),
                    ViskInstruction.PushConst(5),
                    ViskInstruction.PushConst(6),
                    ViskInstruction.PushConst(7),
                    ViskInstruction.PushConst(8),
                    ViskInstruction.PushConst(9),
                    ViskInstruction.PushConst(10),
                    ViskInstruction.Call(of),
                    ViskInstruction.Ret()
                );

                of.AddInstructions(
                    ViskInstruction.SetArg("arg0"),
                    ViskInstruction.SetArg("arg1"),
                    ViskInstruction.SetArg("arg2"),
                    ViskInstruction.SetArg("arg3"),
                    ViskInstruction.SetArg("arg4"),
                    ViskInstruction.SetArg("arg5"),
                    ViskInstruction.SetArg("arg6"),
                    ViskInstruction.SetArg("arg7"),
                    ViskInstruction.SetArg("arg8"),
                    ViskInstruction.SetArg("arg9"),
                    ViskInstruction.LoadLocal("arg0"),
                    ViskInstruction.LoadLocal("arg1"),
                    ViskInstruction.IMul(),
                    ViskInstruction.LoadLocal("arg2"),
                    ViskInstruction.IMul(),
                    ViskInstruction.LoadLocal("arg3"),
                    ViskInstruction.IMul(),
                    ViskInstruction.LoadLocal("arg4"),
                    ViskInstruction.IMul(),
                    ViskInstruction.LoadLocal("arg5"),
                    ViskInstruction.IMul(),
                    ViskInstruction.LoadLocal("arg6"),
                    ViskInstruction.IMul(),
                    ViskInstruction.LoadLocal("arg7"),
                    ViskInstruction.IMul(),
                    ViskInstruction.LoadLocal("arg8"),
                    ViskInstruction.IMul(),
                    ViskInstruction.LoadLocal("arg9"),
                    ViskInstruction.IMul(),
                    ViskInstruction.Ret()
                );
            }
        );

        Assert.That(result, Is.EqualTo(1 * 2 * 3 * 4 * 5 * 6 * 7 * 8 * 9 * 10));
    }

    [Test]
    public void Test13()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConst(1),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(3),
                ViskInstruction.PushConst(4),
                ViskInstruction.PushConst(5),
                ViskInstruction.PushConst(6),
                ViskInstruction.PushConst(7),
                ViskInstruction.PushConst(8),
                ViskInstruction.PushConst(9),
                ViskInstruction.PushConst(10),
                ViskInstruction.CallForeign(_bigMethodInfo),

                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(1 * 2 * 3 * 4 * 5 / 6 * 7 * 8 * 9 * 10));
    }

    [Test]
    public void Test14()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConst(1),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(3),
                ViskInstruction.CallForeign(_smallMethodInfo),

                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(1 / 2 * 3));
    }

    [Test]
    public void Test15()
    {
        var result = ExecuteFunctions(
            module =>
            {
                var mf = module.AddFunction("main", new List<Type>(0), ViskConsts.I64);
                var of = module.AddFunction("other", new List<Type> { ViskConsts.I64 }, ViskConsts.I64);

                mf.AddInstructions(
                    ViskInstruction.PushConst(6),
                    ViskInstruction.Call(of),
                    ViskInstruction.Ret()
                );

                of.AddInstructions(
                    ViskInstruction.SetArg("arg"),
                    ViskInstruction.LoadLocal("arg"),
                    ViskInstruction.PushConst(100),
                    ViskInstruction.Equals(),
                    ViskInstruction.IfTrue(ViskInstruction.Goto("endOfRecursion")),
                    ViskInstruction.LoadLocal("arg"),
                    ViskInstruction.PushConst(1),
                    ViskInstruction.Add(),
                    ViskInstruction.SetLocal("arg"),
                    ViskInstruction.LoadLocal("arg"),
                    ViskInstruction.Call(of),
                    ViskInstruction.SetLocal("arg"),
                    ViskInstruction.SetLabel("endOfRecursion"),
                    ViskInstruction.LoadLocal("arg"),
                    ViskInstruction.Ret()
                );
            }
        );

        Assert.That(result, Is.EqualTo(100));
    }


    [Test]
    public void Test16()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.CallForeign(_voidMethodInfo),

                ViskInstruction.PushConst(-1),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(-1));
    }


    [Test]
    public void Test17()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConst(-1),
                ViskInstruction.CallForeign(_voidMethodInfo),

                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(-1));
    }


    [Test]
    public void Test18()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.CallForeign(_doubleToLongMethodInfo),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(DoubleToLong(123.321)));
    }


    [Test]
    public void Test19()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.123),
                ViskInstruction.AddD(),
                ViskInstruction.CallForeign(_doubleToLongMethodInfo),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(DoubleToLong(123.321 + 123.123)));
    }


    [Test]
    public void Test20()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.123),
                ViskInstruction.MulD(),
                ViskInstruction.CallForeign(_doubleToLongMethodInfo),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(DoubleToLong(123.321 * 123.123)));
    }


    [Test]
    public void Test21()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.123),
                ViskInstruction.SubD(),
                ViskInstruction.CallForeign(_doubleToLongMethodInfo),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(DoubleToLong(123.321 - 123.123)));
    }


    [Test]
    public void Test22()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.123),
                ViskInstruction.DivD(),
                ViskInstruction.CallForeign(_doubleToLongMethodInfo),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(DoubleToLong(123.321 / 123.123)));
    }


    [Test]
    public void Test23()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.123),
                ViskInstruction.DivD(),
                ViskInstruction.CallForeign(_doubleToLongMethodInfo),

                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(DoubleToLong(123.321 / 123.123)));
    }


    [Test]
    public void Test24()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.321),
                ViskInstruction.PushConstD(123.123),
                ViskInstruction.MulD(),
                ViskInstruction.DivD(),
                ViskInstruction.SubD(),
                ViskInstruction.AddD(),
                ViskInstruction.CallForeign(_doubleToLongMethodInfo),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(DoubleToLong(123.321 + (123.321 - 123.321 / (123.321 * 123.123)))));
    }


    [Test]
    public void Test25()
    {
        var result = ExecuteFunctions(
            module =>
            {
                var mf = module.AddFunction("main", new List<Type>(), ViskConsts.I64);
                var of = module.AddFunction("other", new List<Type> { ViskConsts.F64 }, ViskConsts.F64);

                mf.AddInstructions(ViskInstruction.PushConstD(321.123), ViskInstruction.Call(of),
                    ViskInstruction.Call(of), ViskInstruction.PushConstD(22.33), ViskInstruction.MulD(),
                    ViskInstruction.CallForeign(_doubleToLongMethodInfo), ViskInstruction.Ret());

                of.AddInstructions(ViskInstruction.SetArgD("i"), ViskInstruction.LoadLocalD("i"),
                    ViskInstruction.PushConstD(2.1), ViskInstruction.DivD(), ViskInstruction.RetD());
            }
        );

        Assert.That(result, Is.EqualTo(DoubleToLong(321.123 / 2.1 / 2.1 * 22.33)));
    }


    [Test]
    public void Test27()
    {
        var result = ExecuteFunctions(
            module =>
            {
                var mf = module.AddFunction("main", new List<Type>(), ViskConsts.I64);
                var of = module.AddFunction("other", Enumerable.Repeat(ViskConsts.F64, 15).ToList(), ViskConsts.F64);

                mf.AddInstructions(ViskInstruction.PushConstD(1), ViskInstruction.PushConstD(2),
                    ViskInstruction.PushConstD(3), ViskInstruction.PushConstD(4), ViskInstruction.PushConstD(5),
                    ViskInstruction.PushConstD(6), ViskInstruction.PushConstD(7), ViskInstruction.PushConstD(8),
                    ViskInstruction.PushConstD(9), ViskInstruction.PushConstD(10), ViskInstruction.PushConstD(11),
                    ViskInstruction.PushConstD(12), ViskInstruction.PushConstD(13), ViskInstruction.PushConstD(14),
                    ViskInstruction.PushConstD(15), ViskInstruction.Call(of),
                    ViskInstruction.CallForeign(_doubleToLongMethodInfo), ViskInstruction.Ret());

                of.AddInstructions(ViskInstruction.SetArgD("i0"), ViskInstruction.SetArgD("i1"),
                    ViskInstruction.SetArgD("i2"), ViskInstruction.SetArgD("i3"), ViskInstruction.SetArgD("i4"),
                    ViskInstruction.SetArgD("i5"), ViskInstruction.SetArgD("i6"), ViskInstruction.SetArgD("i7"),
                    ViskInstruction.SetArgD("i8"), ViskInstruction.SetArgD("i9"), ViskInstruction.SetArgD("i10"),
                    ViskInstruction.SetArgD("i11"), ViskInstruction.SetArgD("i12"), ViskInstruction.SetArgD("i13"),
                    ViskInstruction.SetArgD("i14"), ViskInstruction.LoadLocalD("i4"), ViskInstruction.RetD());
            }
        );

        Assert.That(result, Is.EqualTo(DoubleToLong(5)));
    }


    [Test]
    public void Test28()
    {
        var result = ExecuteFunctions(
            module =>
            {
                var mf = module.AddFunction("main", new List<Type>(), ViskConsts.I64);
                var of = module.AddFunction("other", Enumerable.Repeat(ViskConsts.F64, 15).ToList(), ViskConsts.F64);

                mf.AddInstructions(ViskInstruction.PushConstD(1), ViskInstruction.PushConstD(2),
                    ViskInstruction.PushConstD(3), ViskInstruction.PushConstD(4), ViskInstruction.PushConstD(5),
                    ViskInstruction.PushConstD(6), ViskInstruction.PushConstD(7), ViskInstruction.PushConstD(8),
                    ViskInstruction.PushConstD(9), ViskInstruction.PushConstD(10), ViskInstruction.PushConstD(11),
                    ViskInstruction.PushConstD(12), ViskInstruction.PushConstD(13), ViskInstruction.PushConstD(14),
                    ViskInstruction.PushConstD(15), ViskInstruction.Call(of),
                    ViskInstruction.CallForeign(_doubleToLongMethodInfo), ViskInstruction.Ret());

                of.AddInstructions(ViskInstruction.SetArgD("i0"), ViskInstruction.SetArgD("i1"),
                    ViskInstruction.SetArgD("i2"), ViskInstruction.SetArgD("i3"), ViskInstruction.SetArgD("i4"),
                    ViskInstruction.SetArgD("i5"), ViskInstruction.SetArgD("i6"), ViskInstruction.SetArgD("i7"),
                    ViskInstruction.SetArgD("i8"), ViskInstruction.SetArgD("i9"), ViskInstruction.SetArgD("i10"),
                    ViskInstruction.SetArgD("i11"), ViskInstruction.SetArgD("i12"), ViskInstruction.SetArgD("i13"),
                    ViskInstruction.SetArgD("i14"), ViskInstruction.LoadLocalD("i14"), ViskInstruction.RetD());
            }
        );

        Assert.That(result, Is.EqualTo(DoubleToLong(15)));
    }


    [Test]
    public void Test29()
    {
        var result = ExecuteFunctions(
            module =>
            {
                var mf = module.AddFunction("main", new List<Type>(), ViskConsts.I64);
                var of = module.AddFunction("other", Enumerable.Repeat(ViskConsts.F64, 15).ToList(), ViskConsts.F64);

                mf.AddInstructions(ViskInstruction.PushConstD(1), ViskInstruction.PushConstD(2),
                    ViskInstruction.PushConstD(3), ViskInstruction.PushConstD(4), ViskInstruction.PushConstD(5),
                    ViskInstruction.PushConstD(6), ViskInstruction.PushConstD(7), ViskInstruction.PushConstD(8),
                    ViskInstruction.PushConstD(9), ViskInstruction.PushConstD(10), ViskInstruction.PushConstD(11),
                    ViskInstruction.PushConstD(12), ViskInstruction.PushConstD(13), ViskInstruction.PushConstD(14),
                    ViskInstruction.PushConstD(15), ViskInstruction.Call(of),
                    ViskInstruction.CallForeign(_doubleToLongMethodInfo), ViskInstruction.Ret());

                of.AddInstructions(ViskInstruction.SetArgD("i0"), ViskInstruction.SetArgD("i1"),
                    ViskInstruction.SetArgD("i2"), ViskInstruction.SetArgD("i3"), ViskInstruction.SetArgD("i4"),
                    ViskInstruction.SetArgD("i5"), ViskInstruction.SetArgD("i6"), ViskInstruction.SetArgD("i7"),
                    ViskInstruction.SetArgD("i8"), ViskInstruction.SetArgD("i9"), ViskInstruction.SetArgD("i10"),
                    ViskInstruction.SetArgD("i11"), ViskInstruction.SetArgD("i12"), ViskInstruction.SetArgD("i13"),
                    ViskInstruction.SetArgD("i14"), ViskInstruction.LoadLocalD("i0"), ViskInstruction.RetD());
            }
        );

        Assert.That(result, Is.EqualTo(DoubleToLong(1)));
    }


    [Test]
    public void Test30()
    {
        var result = ExecuteFunctions(
            module =>
            {
                var mf = module.AddFunction("main", new List<Type>(), ViskConsts.I64);
                var of = module.AddFunction("other", Enumerable.Repeat(ViskConsts.F64, 15).ToList(), ViskConsts.F64);

                mf.AddInstructions(ViskInstruction.PushConstD(10), ViskInstruction.PushConstD(10),
                    ViskInstruction.PushConstD(10), ViskInstruction.PushConstD(10), ViskInstruction.PushConstD(10),
                    ViskInstruction.PushConstD(10), ViskInstruction.PushConstD(10), ViskInstruction.PushConstD(10),
                    ViskInstruction.PushConstD(10), ViskInstruction.PushConstD(10), ViskInstruction.PushConstD(10),
                    ViskInstruction.PushConstD(10), ViskInstruction.PushConstD(10), ViskInstruction.PushConstD(10),
                    ViskInstruction.PushConstD(10), ViskInstruction.Call(of),
                    ViskInstruction.CallForeign(_doubleToLongMethodInfo), ViskInstruction.Ret());

                of.AddInstructions(ViskInstruction.SetArgD("i0"), ViskInstruction.SetArgD("i1"),
                    ViskInstruction.SetArgD("i2"), ViskInstruction.SetArgD("i3"), ViskInstruction.SetArgD("i4"),
                    ViskInstruction.SetArgD("i5"), ViskInstruction.SetArgD("i6"), ViskInstruction.SetArgD("i7"),
                    ViskInstruction.SetArgD("i8"), ViskInstruction.SetArgD("i9"), ViskInstruction.SetArgD("i10"),
                    ViskInstruction.SetArgD("i11"), ViskInstruction.SetArgD("i12"), ViskInstruction.SetArgD("i13"),
                    ViskInstruction.SetArgD("i14"), ViskInstruction.LoadLocalD("i0"), ViskInstruction.LoadLocalD("i1"),
                    ViskInstruction.LoadLocalD("i2"), ViskInstruction.LoadLocalD("i3"),
                    ViskInstruction.LoadLocalD("i4"), ViskInstruction.LoadLocalD("i5"),
                    ViskInstruction.LoadLocalD("i6"), ViskInstruction.LoadLocalD("i7"),
                    ViskInstruction.LoadLocalD("i8"), ViskInstruction.LoadLocalD("i9"),
                    ViskInstruction.LoadLocalD("i10"), ViskInstruction.LoadLocalD("i11"),
                    ViskInstruction.LoadLocalD("i12"), ViskInstruction.LoadLocalD("i13"),
                    ViskInstruction.LoadLocalD("i14"), ViskInstruction.CallForeign(_sum15DoublesMethodInfo),
                    ViskInstruction.RetD());
            }
        );

        Assert.That(result, Is.EqualTo(DoubleToLong(150)));
    }

    [Test]
    public void Test31()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConstD(5.321),
                ViskInstruction.PushConstD(3.321),
                ViskInstruction.DropD(),

                ViskInstruction.CallForeign(_doubleToLongMethodInfo),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(DoubleToLong(5.321)));
    }

    [Test]
    public void Test32()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConstD(5.321),
                ViskInstruction.DupD(),
                ViskInstruction.MulD(),

                ViskInstruction.CallForeign(_doubleToLongMethodInfo),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(DoubleToLong(5.321 * 5.321)));
    }

    [Test]
    public void Test33()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConst(5),
                ViskInstruction.Dup(),
                ViskInstruction.IMul(),

                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(5 * 5));
    }

    [Test]
    public void Test34()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConst(5),
                ViskInstruction.PushConst(6),
                ViskInstruction.Drop(),

                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(5));
    }

    [Test]
    public void Test35()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConst(5),
                ViskInstruction.SetLocal("i"),

                ViskInstruction.LoadLocal("i"),
                ViskInstruction.PushConst(10),
                ViskInstruction.LessThan(),

                ViskInstruction.IfTrue(
                    new List<ViskInstruction>
                    {
                        ViskInstruction.PushConst(10)
                    },
                    new List<ViskInstruction>
                    {
                        ViskInstruction.PushConst(-10)
                    }
                ),

                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(10));
    }

    [Test]
    public void Test36()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConst(10),
                ViskInstruction.SetLocal("i"),

                ViskInstruction.LoadLocal("i"),
                ViskInstruction.PushConst(10),
                ViskInstruction.LessThan(),

                ViskInstruction.IfTrue(
                    new List<ViskInstruction>
                    {
                        ViskInstruction.PushConst(10)
                    },
                    new List<ViskInstruction>
                    {
                        ViskInstruction.PushConst(-10)
                    }
                ),

                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(-10));
    }

    [Test]
    public void Test37()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConst(0),
                ViskInstruction.SetLocal("i"),

                ViskInstruction.LoadLocal("i"),
                ViskInstruction.PushConst(10),
                ViskInstruction.GreaterThan(),

                ViskInstruction.IfTrue(
                    new List<ViskInstruction>
                    {
                        ViskInstruction.PushConst(10)
                    },
                    new List<ViskInstruction>
                    {
                        ViskInstruction.LoadLocal("i"),
                        ViskInstruction.PushConst(5),
                        ViskInstruction.GreaterThan(),

                        ViskInstruction.IfTrue(
                            new List<ViskInstruction>
                            {
                                ViskInstruction.PushConst(5)
                            },
                            new List<ViskInstruction>
                            {
                                ViskInstruction.PushConst(0)
                            }
                        )
                    }
                ),

                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void Test38()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConst(6),
                ViskInstruction.SetLocal("i"),

                ViskInstruction.LoadLocal("i"),
                ViskInstruction.PushConst(10),
                ViskInstruction.GreaterThan(),

                ViskInstruction.IfTrue(
                    new List<ViskInstruction>
                    {
                        ViskInstruction.PushConst(10)
                    },
                    new List<ViskInstruction>
                    {
                        ViskInstruction.LoadLocal("i"),
                        ViskInstruction.PushConst(5),
                        ViskInstruction.GreaterThan(),

                        ViskInstruction.IfTrue(
                            new List<ViskInstruction>
                            {
                                ViskInstruction.PushConst(5)
                            },
                            new List<ViskInstruction>
                            {
                                ViskInstruction.PushConst(0)
                            }
                        )
                    }
                ),

                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(5));
    }

    [Test]
    public void Test39()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConstD(123),
                ViskInstruction.PushConstD(321),
                ViskInstruction.LessThanD(),

                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public void Test40()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConstD(321),
                ViskInstruction.PushConstD(321),
                ViskInstruction.LessThanD(),

                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void Test41()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConstD(321),
                ViskInstruction.PushConstD(321),
                ViskInstruction.GreaterThanD(),

                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void Test42()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConstD(322),
                ViskInstruction.PushConstD(321),
                ViskInstruction.GreaterThanD(),

                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public void Test43()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConstD(321),
                ViskInstruction.PushConstD(321),
                ViskInstruction.GreaterThanOrEqualsD(),

                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public void Test44()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConstD(321),
                ViskInstruction.PushConstD(321),
                ViskInstruction.LessThanOrEqualsD(),

                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public void Test45()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConstD(321),
                ViskInstruction.PushConstD(321),
                ViskInstruction.EqualsD(),

                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public void Test46()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConstD(321),
                ViskInstruction.PushConstD(321),
                ViskInstruction.NotEqualsD(),

                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void Test47()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConstD(123),
                ViskInstruction.PushConstD(123),
                ViskInstruction.PushConstD(123),
                ViskInstruction.PushConstD(123),
                ViskInstruction.PushConstD(123),
                ViskInstruction.PushConstD(123),
                ViskInstruction.PushConstD(123),
                ViskInstruction.PushConstD(123),
                ViskInstruction.PushConstD(123),
                ViskInstruction.PushConstD(123),
                ViskInstruction.PushConstD(123),
                ViskInstruction.PushConstD(123),
                ViskInstruction.PushConstD(123),
                ViskInstruction.PushConstD(123),
                ViskInstruction.PushConstD(123),
                ViskInstruction.PushConstD(123),
                ViskInstruction.PushConstD(321),
                ViskInstruction.LessThanD(),

                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public void Test48()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConst(1),
                ViskInstruction.SetLocal("i"),

                ViskInstruction.PushConst(2),
                ViskInstruction.LoadRef("i"),
                ViskInstruction.SetByRef(),

                ViskInstruction.LoadLocal("i"),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(2));
    }

    [Test]
    public void Test49()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConst(1),
                ViskInstruction.PushConst(1),
                ViskInstruction.PushConst(1),
                ViskInstruction.PushConst(1),
                ViskInstruction.PushConst(1),
                ViskInstruction.PushConst(1),
                ViskInstruction.PushConst(1),
                ViskInstruction.PushConst(1),
                ViskInstruction.PushConst(1),
                ViskInstruction.PushConst(1),
                ViskInstruction.PushConst(1),
                ViskInstruction.PushConst(1),
                ViskInstruction.PushConst(1),
                ViskInstruction.PushConst(1),
                ViskInstruction.PushConst(1),
                ViskInstruction.PushConst(1),
                ViskInstruction.PushConst(1),
                ViskInstruction.PushConst(1),
                ViskInstruction.PushConst(1),
                ViskInstruction.PushConst(1),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal("i"),

                ViskInstruction.PushConst(2),
                ViskInstruction.LoadRef("i"),
                ViskInstruction.SetByRef(),

                ViskInstruction.LoadLocal("i"),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(2));
    }

    [Test]
    public void Test50()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConstD(1),
                ViskInstruction.PushConstD(1),
                ViskInstruction.PushConstD(1),
                ViskInstruction.PushConstD(1),
                ViskInstruction.PushConstD(1),
                ViskInstruction.PushConstD(1),
                ViskInstruction.PushConstD(1),
                ViskInstruction.PushConstD(1),
                ViskInstruction.PushConstD(1),
                ViskInstruction.PushConstD(1),
                ViskInstruction.PushConstD(1),
                ViskInstruction.PushConstD(1),
                ViskInstruction.PushConstD(1),
                ViskInstruction.PushConstD(1),
                ViskInstruction.PushConstD(1),
                ViskInstruction.PushConstD(1),
                ViskInstruction.PushConstD(1),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD("i"),

                ViskInstruction.PushConstD(2.123),
                ViskInstruction.LoadRef("i"),
                ViskInstruction.SetByRefD(),

                ViskInstruction.LoadLocalD("i"),
                ViskInstruction.CallForeign(_doubleToLongMethodInfo),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(DoubleToLong(2.123)));
    }

    [Test]
    public void Test51()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConstD(1),
                ViskInstruction.SetLocalD("i"),

                ViskInstruction.PushConstD(2.123),
                ViskInstruction.LoadRef("i"),
                ViskInstruction.SetByRefD(),

                ViskInstruction.LoadLocalD("i"),
                ViskInstruction.CallForeign(_doubleToLongMethodInfo),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(DoubleToLong(2.123)));
    }

    [Test]
    public void Test52()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConstD(2.123),
                ViskInstruction.SetLocalD("i"),

                ViskInstruction.LoadRef("i"),
                ViskInstruction.SetLocal("ref"),

                ViskInstruction.LoadLocal("ref"),
                ViskInstruction.LoadByRefD(),
                ViskInstruction.CallForeign(_doubleToLongMethodInfo),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(DoubleToLong(2.123)));
    }

    [Test]
    public void Test53()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConstD(2.123),
                ViskInstruction.PushConstD(2.123),
                ViskInstruction.PushConstD(2.123),
                ViskInstruction.PushConstD(2.123),
                ViskInstruction.PushConstD(2.123),
                ViskInstruction.PushConstD(2.123),
                ViskInstruction.PushConstD(2.123),
                ViskInstruction.PushConstD(2.123),
                ViskInstruction.PushConstD(2.123),
                ViskInstruction.PushConstD(2.123),
                ViskInstruction.PushConstD(2.123),
                ViskInstruction.PushConstD(2.123),
                ViskInstruction.PushConstD(2.123),
                ViskInstruction.PushConstD(2.123),
                ViskInstruction.PushConstD(2.123),
                ViskInstruction.PushConstD(2.123),
                ViskInstruction.PushConstD(2.123),
                ViskInstruction.PushConstD(2.123),
                ViskInstruction.PushConstD(2.123),
                ViskInstruction.PushConstD(2.123),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocalD("i"),

                ViskInstruction.LoadRef("i"),
                ViskInstruction.SetLocal("ref"),

                ViskInstruction.LoadLocal("ref"),
                ViskInstruction.LoadByRefD(),
                ViskInstruction.CallForeign(_doubleToLongMethodInfo),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(DoubleToLong(2.123)));
    }

    [Test]
    public void Test54()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConst(2),
                ViskInstruction.SetLocal("i"),

                ViskInstruction.LoadRef("i"),
                ViskInstruction.SetLocal("ref"),

                ViskInstruction.LoadLocal("ref"),
                ViskInstruction.LoadByRef(),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(2));
    }

    [Test]
    public void Test55()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(2),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal($"i{Guid.NewGuid()}"),
                ViskInstruction.SetLocal("i"),

                ViskInstruction.LoadRef("i"),
                ViskInstruction.SetLocal("ref"),

                ViskInstruction.LoadLocal("ref"),
                ViskInstruction.LoadByRef(),
                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(2));
    }

    private static long ExecuteFunctions(IEnumerable<ViskInstruction> instructions) =>
        ExecuteFunctions(instructions.ToArray());

    private static long ExecuteFunctions(params ViskInstruction[] instructions)
    {
        var module = new ViskModule("main");
        var mf = module.AddFunction("main", new List<Type>(0), ViskConsts.I64);

        mf.AddInstructions(instructions);

        var image = new ViskImage(module);
        var executor = image.Compile(new ViskSettings(ViskCompilationMode.Release));
        var asmDelegate = executor.GetDelegate();

        return asmDelegate();
    }

    private static long ExecuteFunctions(Action<ViskModule> builder)
    {
        var module = new ViskModule("main");

        builder(module);

        var image = new ViskImage(module);
        var executor = image.Compile(new ViskSettings(ViskCompilationMode.Release));
        var asmDelegate = executor.GetDelegate();

        return asmDelegate();
    }

    private static long SmallMethod(long l0, long l1, long l2) => l0 / l1 * l2;

    private static long BigMethod(
        long l0, long l1, long l2, long l3, long l4, long l5, long l6, long l7, long l8, long l9
    ) => l0 * l1 * l2 * l3 * l4 / l5 * l6 * l7 * l8 * l9;

    private static double Sum15Doubles(
        double l0, double l1, double l2, double l3, double l4, double l5, double l6, double l7, double l8, double l9,
        double l10, double l11, double l12, double l13, double l14
    ) => l0 + l1 + l2 + l3 + l4 + l5 + l6 + l7 + l8 + l9 + l10 + l11 + l12 + l13 + l14;

    private static void VoidMethod() => Console.WriteLine("hi from void method");

    private static long DoubleToLong(double value) => BitConverter.DoubleToInt64Bits(value);
}