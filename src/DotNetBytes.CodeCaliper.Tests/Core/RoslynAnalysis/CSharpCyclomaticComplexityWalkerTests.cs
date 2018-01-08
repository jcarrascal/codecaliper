namespace DotNetBytes.CodeCaliper.Tests.Core.RoslynAnalysis
{
    using System.Dynamic;
    using DotNetBytes.CodeCaliper.Core;
    using DotNetBytes.CodeCaliper.Core.RoslynAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Xunit;

    public class CSharpCyclomaticComplexityWalkerTests
    {
        private static ProcessContext WalkSourceCode(string fileId, string sourceCode)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var context = new ProcessContext();
            var walker = new CSharpCyclomaticComplexityWalker(context);
            dynamic file = new ExpandoObject();
            file.Identifier = fileId;
            context[fileId] = file;

            walker.Visit(file, syntaxTree);

            return context;
        }

        [Fact]
        public void Visit_WhenGivenAClassWithAConstructorAndDestructor_ThenRegistersTheMethodWithComplexity2()
        {
            var sourceCode = @"using System.Linq;
            class Foo
            {
                public Foo()
                {
                }
                ~Foo()
                {
                }
            }";
            var context = WalkSourceCode("Foo.cs", sourceCode);

            Assert.Equal(2, context["Foo.cs:Foo"].CyclomaticComplexity);

            Assert.True(context.ContainsKey("Foo.cs:Foo:Foo()"));
            Assert.Equal(1, context["Foo.cs:Foo:Foo()"].CyclomaticComplexity);
            Assert.Equal(0, context["Foo.cs:Foo:Foo()"].SourceLinesOfCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo:~Foo()"));
            Assert.Equal(1, context["Foo.cs:Foo:~Foo()"].CyclomaticComplexity);
            Assert.Equal(0, context["Foo.cs:Foo:~Foo()"].SourceLinesOfCode);
        }

        [Fact]
        public void Visit_WhenGivenAClassWithAnIndexer_ThenEachGetterAndSetterIncrementsComplexityBy1()
        {
            var sourceCode = @"class Foo
            {
                public int this[int i]
                {
                    get { return 1; }
                    set { }
                }
            }";
            var context = WalkSourceCode("Foo.cs", sourceCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo"));
            Assert.Equal(2, context["Foo.cs:Foo"].CyclomaticComplexity);
            Assert.Equal(1, context["Foo.cs:Foo"].SourceLinesOfCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo:[int]int:get"));
            Assert.Equal(1, context["Foo.cs:Foo:[int]int:get"].CyclomaticComplexity);
            Assert.Equal(1, context["Foo.cs:Foo:[int]int:get"].SourceLinesOfCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo:[int]int:set"));
            Assert.Equal(1, context["Foo.cs:Foo:[int]int:set"].CyclomaticComplexity);
            Assert.Equal(0, context["Foo.cs:Foo:[int]int:set"].SourceLinesOfCode);
        }

        [Fact]
        public void Visit_WhenGivenAClassWithAProperty_ThenEachGetterAndSetterIncrementsComplexityBy1()
        {
            var sourceCode = @"class Foo
            {
                private string property;
                public string Property
                {
                    get { return property; }
                    set { property = value; }
                }
            }";
            var context = WalkSourceCode("Foo.cs", sourceCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo"));
            Assert.Equal(2, context["Foo.cs:Foo"].CyclomaticComplexity);
            Assert.Equal(2, context["Foo.cs:Foo"].SourceLinesOfCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo:Property:get"));
            Assert.Equal(1, context["Foo.cs:Foo:Property:get"].CyclomaticComplexity);
            Assert.Equal(1, context["Foo.cs:Foo:Property:get"].SourceLinesOfCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo:Property:set"));
            Assert.Equal(1, context["Foo.cs:Foo:Property:set"].CyclomaticComplexity);
            Assert.Equal(1, context["Foo.cs:Foo:Property:set"].SourceLinesOfCode);
        }

        [Fact]
        public void Visit_WhenGivenAMethodWithADelegateExpression_ThenRegistersTheMethodWithComplexity2()
        {
            var sourceCode = @"using System.Linq;
            class Foo
            {
                void Bar()
                {
                    string.Join(string.Empty, System.Environment.UserName.Select(delegate (char c) 
                        {
                            return char.ToUpper(c);
                        }));
                }
            }";
            var context = WalkSourceCode("Foo.cs", sourceCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo"));
            Assert.Equal(2, context["Foo.cs:Foo"].CyclomaticComplexity);
            Assert.Equal(2, context["Foo.cs:Foo"].SourceLinesOfCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo:Bar()void"));
            Assert.Equal(2, context["Foo.cs:Foo:Bar()void"].CyclomaticComplexity);
            Assert.Equal(2, context["Foo.cs:Foo:Bar()void"].SourceLinesOfCode);
        }

        [Fact]
        public void Visit_WhenGivenAMethodWithADoWhileStatement_ThenRegistersTheMethodWithComplexity2()
        {
            var sourceCode = @"class Foo
            {
                void Bar()
                {
                    do
                    {
                        System.Console.Out.Flush();
                    }
                    while (1 == 1);
                    System.Console.ReadKey();
                }
            }";
            var context = WalkSourceCode("Foo.cs", sourceCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo"));
            Assert.Equal(2, context["Foo.cs:Foo"].CyclomaticComplexity);
            Assert.Equal(3, context["Foo.cs:Foo"].SourceLinesOfCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo:Bar()void"));
            Assert.Equal(2, context["Foo.cs:Foo:Bar()void"].CyclomaticComplexity);
            Assert.Equal(3, context["Foo.cs:Foo:Bar()void"].SourceLinesOfCode);
        }

        [Fact]
        public void Visit_WhenGivenAMethodWithADoWhileStatementWithTwoConditions_ThenRegistersTheMethodWithComplexity2()
        {
            var sourceCode = @"class Foo
            {
                void Bar()
                {
                    do
                    {
                        System.Console.Out.Flush();
                    }
                    while (1 == 1 && 2 == 2);
                    System.Console.ReadKey();
                }
            }";
            var context = WalkSourceCode("Foo.cs", sourceCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo"));
            Assert.Equal(3, context["Foo.cs:Foo"].CyclomaticComplexity);
            Assert.Equal(3, context["Foo.cs:Foo"].SourceLinesOfCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo:Bar()void"));
            Assert.Equal(3, context["Foo.cs:Foo:Bar()void"].CyclomaticComplexity);
            Assert.Equal(3, context["Foo.cs:Foo:Bar()void"].SourceLinesOfCode);
        }

        [Fact]
        public void Visit_WhenGivenAMethodWithAForeachStatement_ThenRegistersTheMethodWithComplexity2()
        {
            var sourceCode = @"class Foo
            {
                void Bar()
                {
                    foreach (char c in Environment.User)
                    {
                        System.Console.Out.Flush();
                    }
                    System.Console.ReadKey();
                }
            }";
            var context = WalkSourceCode("Foo.cs", sourceCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo"));
            Assert.Equal(2, context["Foo.cs:Foo"].CyclomaticComplexity);
            Assert.Equal(3, context["Foo.cs:Foo"].SourceLinesOfCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo:Bar()void"));
            Assert.Equal(2, context["Foo.cs:Foo:Bar()void"].CyclomaticComplexity);
            Assert.Equal(3, context["Foo.cs:Foo:Bar()void"].SourceLinesOfCode);
        }

        [Fact]
        public void Visit_WhenGivenAMethodWithAForStatement_ThenRegistersTheMethodWithComplexity2()
        {
            var sourceCode = @"class Foo
            {
                void Bar()
                {
                    for (int i = 0; i < 10; i++)
                    {
                        System.Console.Out.Flush();
                    }
                    System.Console.ReadKey();
                }
            }";
            var context = WalkSourceCode("Foo.cs", sourceCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo"));
            Assert.Equal(2, context["Foo.cs:Foo"].CyclomaticComplexity);
            Assert.Equal(3, context["Foo.cs:Foo"].SourceLinesOfCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo:Bar()void"));
            Assert.Equal(2, context["Foo.cs:Foo:Bar()void"].CyclomaticComplexity);
            Assert.Equal(3, context["Foo.cs:Foo:Bar()void"].SourceLinesOfCode);
        }

        [Fact]
        public void Visit_WhenGivenAMethodWithALambdaExpression_ThenRegistersTheMethodWithComplexity2()
        {
            var sourceCode = @"using System.Linq;
            class Foo
            {
                void Bar()
                {
                    string.Join(string.Empty, Environment.User.Select(c => c.ToUpper()));
                }
            }";
            var context = WalkSourceCode("Foo.cs", sourceCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo"));
            Assert.Equal(2, context["Foo.cs:Foo"].CyclomaticComplexity);
            Assert.Equal(2, context["Foo.cs:Foo"].SourceLinesOfCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo:Bar()void"));
            Assert.Equal(2, context["Foo.cs:Foo:Bar()void"].CyclomaticComplexity);
            Assert.Equal(2, context["Foo.cs:Foo:Bar()void"].SourceLinesOfCode);
        }

        [Fact]
        public void Visit_WhenGivenAMethodWithAnIfStatement_ThenRegistersTheMethodWithComplexity2()
        {
            var sourceCode = @"class Foo<TInput, TOutput>
            {
                void Bar()
                {
                    if (1 == 1)
                    {
                        System.Console.Out.Flush();
                    }
                    System.Console.ReadKey();
                }
            }";
            var context = WalkSourceCode("Foo.cs", sourceCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo<TInput, TOutput>"));
            Assert.Equal(2, context["Foo.cs:Foo<TInput, TOutput>"].CyclomaticComplexity);
            Assert.Equal(3, context["Foo.cs:Foo<TInput, TOutput>"].SourceLinesOfCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo<TInput, TOutput>:Bar()void"));
            Assert.Equal(2, context["Foo.cs:Foo<TInput, TOutput>:Bar()void"].CyclomaticComplexity);
            Assert.Equal(3, context["Foo.cs:Foo<TInput, TOutput>:Bar()void"].SourceLinesOfCode);
        }

        [Fact]
        public void Visit_WhenGivenAMethodWithAnIfStatementWithTwoConditions_ThenRegistersTheMethodWithComplexity3()
        {
            var sourceCode = @"class Foo<TInput, TOutput>
            {
                void Bar()
                {
                    if (1 == 1 && 2 == 2)
                    {
                        System.Console.Out.Flush();
                    }
                    System.Console.ReadKey();
                }
            }";
            var context = WalkSourceCode("Foo.cs", sourceCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo<TInput, TOutput>"));
            Assert.Equal(3, context["Foo.cs:Foo<TInput, TOutput>"].CyclomaticComplexity);
            Assert.Equal(3, context["Foo.cs:Foo<TInput, TOutput>"].SourceLinesOfCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo<TInput, TOutput>:Bar()void"));
            Assert.Equal(3, context["Foo.cs:Foo<TInput, TOutput>:Bar()void"].CyclomaticComplexity);
            Assert.Equal(3, context["Foo.cs:Foo<TInput, TOutput>:Bar()void"].SourceLinesOfCode);
        }

        [Fact]
        public void Visit_WhenGivenAMethodWithASinglePath_ThenRegistersTheMethodWithComplexity1()
        {
            var sourceCode = @"class Foo<TInput, TOutput>
            {
                void Bar()
                {
                    System.Console.ReadKey();
                }
            }";
            var context = WalkSourceCode("Foo.cs", sourceCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo<TInput, TOutput>"));
            Assert.Equal(1, context["Foo.cs:Foo<TInput, TOutput>"].CyclomaticComplexity);
            Assert.Equal(1, context["Foo.cs:Foo<TInput, TOutput>"].SourceLinesOfCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo<TInput, TOutput>:Bar()void"));
            Assert.Equal(1, context["Foo.cs:Foo<TInput, TOutput>:Bar()void"].CyclomaticComplexity);
            Assert.Equal(1, context["Foo.cs:Foo<TInput, TOutput>:Bar()void"].SourceLinesOfCode);
        }

        [Fact]
        public void Visit_WhenGivenAMethodWithASwitchStatement_ThenEachCaseIncrementsTheMethodComplexityBy1()
        {
            var sourceCode = @"class Foo
            {
                void Bar()
                {
                    switch(Environment.User)
                    {
                        case ""foo"":
                            break;
                        case ""bar"":
                            break;
                        default:
                            System.Console.Out.Flush();
                            break;
                    }
                    System.Console.ReadKey();
                }
            }";
            var context = WalkSourceCode("Foo.cs", sourceCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo"));
            Assert.Equal(3, context["Foo.cs:Foo"].CyclomaticComplexity);
            Assert.Equal(6, context["Foo.cs:Foo"].SourceLinesOfCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo:Bar()void"));
            Assert.Equal(3, context["Foo.cs:Foo:Bar()void"].CyclomaticComplexity);
            Assert.Equal(6, context["Foo.cs:Foo:Bar()void"].SourceLinesOfCode);
        }

        [Fact]
        public void Visit_WhenGivenAMethodWithASwitchStatementWithJustTheDefault_ThenRegistersTheMethodWithComplexity1()
        {
            var sourceCode = @"class Foo
            {
                void Bar()
                {
                    switch(Environment.User)
                    {
                        default:
                            System.Console.Out.Flush();
                            break;
                    }
                    System.Console.ReadKey();
                }
            }";
            var context = WalkSourceCode("Foo.cs", sourceCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo"));
            Assert.Equal(1, context["Foo.cs:Foo"].CyclomaticComplexity);
            Assert.Equal(4, context["Foo.cs:Foo"].SourceLinesOfCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo:Bar()void"));
            Assert.Equal(1, context["Foo.cs:Foo:Bar()void"].CyclomaticComplexity);
            Assert.Equal(4, context["Foo.cs:Foo:Bar()void"].SourceLinesOfCode);
        }

        [Fact]
        public void Visit_WhenGivenAMethodWithATryCatchStatement_ThenRegistersTheMethodWithComplexity2()
        {
            var sourceCode = @"class Foo
            {
                void Bar()
                {
                    Console.WriteLine(""Before"");
                    try
                    {
                        Console.ReadKey();
                    }
                    catch(System.Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    Console.WriteLine(""After"");
                }
            }";
            var context = WalkSourceCode("Foo.cs", sourceCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo"));
            Assert.Equal(2, context["Foo.cs:Foo"].CyclomaticComplexity);
            Assert.Equal(5, context["Foo.cs:Foo"].SourceLinesOfCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo:Bar()void"));
            Assert.Equal(2, context["Foo.cs:Foo:Bar()void"].CyclomaticComplexity);
            Assert.Equal(5, context["Foo.cs:Foo:Bar()void"].SourceLinesOfCode);
        }

        [Fact]
        public void Visit_WhenGivenAMethodWithAWhileStatement_ThenRegistersTheMethodWithComplexity2()
        {
            var sourceCode = @"class Foo
            {
                void Bar()
                {
                    while (1 == 1)
                    {
                        System.Console.Out.Flush();
                    }
                    System.Console.ReadKey();
                }
            }";
            var context = WalkSourceCode("Foo.cs", sourceCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo"));
            Assert.Equal(2, context["Foo.cs:Foo"].CyclomaticComplexity);
            Assert.Equal(3, context["Foo.cs:Foo"].SourceLinesOfCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo:Bar()void"));
            Assert.Equal(2, context["Foo.cs:Foo:Bar()void"].CyclomaticComplexity);
            Assert.Equal(3, context["Foo.cs:Foo:Bar()void"].SourceLinesOfCode);
        }

        [Fact]
        public void Visit_WhenGivenAMethodWithAWhileStatementWithTwoConditions_ThenRegistersTheMethodWithComplexity3()
        {
            var sourceCode = @"class Foo
            {
                void Bar()
                {
                    while (1 == 1 && 2 == 2)
                    {
                        System.Console.Out.Flush();
                    }
                    System.Console.ReadKey();
                }
            }";
            var context = WalkSourceCode("Foo.cs", sourceCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo"));
            Assert.Equal(3, context["Foo.cs:Foo"].CyclomaticComplexity);
            Assert.Equal(3, context["Foo.cs:Foo"].SourceLinesOfCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo:Bar()void"));
            Assert.Equal(3, context["Foo.cs:Foo:Bar()void"].CyclomaticComplexity);
            Assert.Equal(3, context["Foo.cs:Foo:Bar()void"].SourceLinesOfCode);
        }

        [Fact]
        public void Visit_WhenGivenAMethodWithTheForStatementWithTwoConditions_ThenRegistersTheMethodWithComplexity3()
        {
            var sourceCode = @"class Foo
            {
                void Bar()
                {
                    for (int i = 0; i < 10 || i < 20; i++)
                    {
                        System.Console.Out.Flush();
                    }
                    System.Console.ReadKey();
                }
            }";
            var context = WalkSourceCode("Foo.cs", sourceCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo"));
            Assert.Equal(3, context["Foo.cs:Foo"].CyclomaticComplexity);
            Assert.Equal(3, context["Foo.cs:Foo"].SourceLinesOfCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo:Bar()void"));
            Assert.Equal(3, context["Foo.cs:Foo:Bar()void"].CyclomaticComplexity);
            Assert.Equal(3, context["Foo.cs:Foo:Bar()void"].SourceLinesOfCode);
        }

        [Fact]
        public void Visit_WhenGivenAMethodWithTwoNestedIfStatements_ThenRegistersTheMethodWithComplexity3()
        {
            var sourceCode = @"class Foo<TInput, TOutput>
            {
                void Bar()
                {
                    if (1 == 1)
                    {
                        System.Console.Out.Flush();
                        if (1 == 1)
                        {
                            System.Console.Err.Flush();
                        }
                    }
                    System.Console.ReadKey();
                }
            }";
            var context = WalkSourceCode("Foo.cs", sourceCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo<TInput, TOutput>"));
            Assert.Equal(3, context["Foo.cs:Foo<TInput, TOutput>"].CyclomaticComplexity);
            Assert.Equal(5, context["Foo.cs:Foo<TInput, TOutput>"].SourceLinesOfCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo<TInput, TOutput>:Bar()void"));
            Assert.Equal(3, context["Foo.cs:Foo<TInput, TOutput>:Bar()void"].CyclomaticComplexity);
            Assert.Equal(5, context["Foo.cs:Foo<TInput, TOutput>:Bar()void"].SourceLinesOfCode);
        }

        [Fact]
        public void Visit_WhenGivenTwoMethodsWithASinglePath_ThenRegistersEachMethodWithComplexity1()
        {
            var sourceCode = @"class Foo<TInput, TOutput>
            {
                void Bar()
                {
                    System.Console.ReadKey();
                }
                void Baz<T>()
                {
                    System.Console.ReadKey();
                }
            }";
            var context = WalkSourceCode("Foo.cs", sourceCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo<TInput, TOutput>"));
            Assert.Equal(2, context["Foo.cs:Foo<TInput, TOutput>"].CyclomaticComplexity);
            Assert.Equal(2, context["Foo.cs:Foo<TInput, TOutput>"].SourceLinesOfCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo<TInput, TOutput>:Bar()void"));
            Assert.Equal(1, context["Foo.cs:Foo<TInput, TOutput>:Bar()void"].CyclomaticComplexity);
            Assert.Equal(1, context["Foo.cs:Foo<TInput, TOutput>:Bar()void"].SourceLinesOfCode);

            Assert.True(context.ContainsKey("Foo.cs:Foo<TInput, TOutput>:Baz<T>()void"));
            Assert.Equal(1, context["Foo.cs:Foo<TInput, TOutput>:Baz<T>()void"].CyclomaticComplexity);
            Assert.Equal(1, context["Foo.cs:Foo<TInput, TOutput>:Baz<T>()void"].SourceLinesOfCode);
        }
    }
}