namespace TestProject1;

using System.Reflection;
using ViskCompiler;

public class Tests
{
    public static readonly MethodInfo SmallMethodInfo = typeof(Tests).GetMethod(nameof(SmallMethod))!;
    public static readonly MethodInfo BigMethodInfo = typeof(Tests).GetMethod(nameof(BigMethod))!;

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test00()
    {
        try
        {
            ExecuteFunctions(
                new List<ViskInstruction>
                {
                    ViskInstruction.Ret()
                }
            );
        }
        catch
        {
            Assert.Pass();
        }
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
                ViskInstruction.GotoIfTrue("m"),


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
                ViskInstruction.GotoIfTrue("m"),


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
                var mf = module.AddFunction("main", 0, typeof(long));
                var of = module.AddFunction("other", 0, typeof(long));

                mf.RawInstructions.AddRange(
                    new List<ViskInstruction>
                    {
                        ViskInstruction.Call(of),
                        ViskInstruction.Call(of),
                        ViskInstruction.Add(),

                        ViskInstruction.Ret()
                    }
                );

                of.RawInstructions.AddRange(
                    new List<ViskInstruction>
                    {
                        ViskInstruction.PushConst(23),
                        ViskInstruction.PushConst(2),
                        ViskInstruction.Add(),

                        ViskInstruction.Ret()
                    }
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
                var mf = module.AddFunction("main", 0, typeof(long));
                var of = module.AddFunction("other", 1, typeof(long));

                mf.RawInstructions.AddRange(
                    new List<ViskInstruction>
                    {
                        ViskInstruction.PushConst(5),
                        ViskInstruction.Call(of),
                        ViskInstruction.Call(of),

                        ViskInstruction.Ret()
                    }
                );

                of.RawInstructions.AddRange(
                    new List<ViskInstruction>
                    {
                        ViskInstruction.SetArg("arg"),

                        ViskInstruction.LoadLocal("arg"),
                        ViskInstruction.PushConst(2),
                        ViskInstruction.IMul(),

                        ViskInstruction.Ret()
                    }
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
                var mf = module.AddFunction("main", 0, typeof(long));
                var of = module.AddFunction("other", 10, typeof(long));

                mf.RawInstructions.AddRange(
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
                        ViskInstruction.Call(of),

                        ViskInstruction.Ret()
                    }
                );

                of.RawInstructions.AddRange(
                    new List<ViskInstruction>
                    {
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
                    }
                );
            }
        );

        Assert.That(result, Is.EqualTo(1 * 2 * 3 * 4 * 5 * 6 * 7 * 8 * 9 * 10));
    }

    [Test]
    public void Test13()
    {
        var result = ExecuteFunctions(
            module =>
            {
                var mf = module.AddFunction("main", 0, typeof(long));
                var of = module.AddFunction("other", 10, typeof(long));

                mf.RawInstructions.AddRange(
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
                        ViskInstruction.Call(of),

                        ViskInstruction.Ret()
                    }
                );

                of.RawInstructions.AddRange(
                    new List<ViskInstruction>
                    {
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
                    }
                );
            }
        );

        Assert.That(result, Is.EqualTo(1 * 2 * 3 * 4 * 5 * 6 * 7 * 8 * 9 * 10));
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
                ViskInstruction.PushConst(4),
                ViskInstruction.PushConst(5),
                ViskInstruction.PushConst(6),
                ViskInstruction.PushConst(7),
                ViskInstruction.PushConst(8),
                ViskInstruction.PushConst(9),
                ViskInstruction.PushConst(10),
                ViskInstruction.CallForeign(BigMethodInfo),

                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(1 * 2 * 3 * 4 * 5 / 6 * 7 * 8 * 9 * 10));
    }

    [Test]
    public void Test15()
    {
        var result = ExecuteFunctions(
            new List<ViskInstruction>
            {
                ViskInstruction.PushConst(1),
                ViskInstruction.PushConst(2),
                ViskInstruction.PushConst(3),
                ViskInstruction.CallForeign(SmallMethodInfo),

                ViskInstruction.Ret()
            }
        );

        Assert.That(result, Is.EqualTo(1 / 2 * 3));
    }

    private static long ExecuteFunctions(IEnumerable<ViskInstruction> instructions)
    {
        var module = new ViskModule("main");
        var mf = module.AddFunction("main", 0, typeof(long));

        mf.RawInstructions.AddRange(instructions);

        var image = new ViskImage(module);
        var executor = image.Compile();
        var asmDelegate = executor.GetDelegate();

        return asmDelegate();
    }

    private static long ExecuteFunctions(Action<ViskModule> builder)
    {
        var module = new ViskModule("main");

        builder(module);

        var image = new ViskImage(module);
        var executor = image.Compile();
        var asmDelegate = executor.GetDelegate();

        return asmDelegate();
    }

    public static long SmallMethod(long l0, long l1, long l2) => l0 / l1 * l2;

    public static long BigMethod(
        long l0, long l1, long l2, long l3, long l4, long l5, long l6, long l7, long l8, long l9
    ) => l0 * l1 * l2 * l3 * l4 / l5 * l6 * l7 * l8 * l9;
}