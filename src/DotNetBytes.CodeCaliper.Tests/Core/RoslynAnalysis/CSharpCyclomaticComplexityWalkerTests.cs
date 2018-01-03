namespace DotNetBytes.CodeCaliper.Tests.Core.RoslynAnalysis
{
    using System.Dynamic;
    using DotNetBytes.CodeCaliper.Core;
    using DotNetBytes.CodeCaliper.Core.RoslynAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Xunit;

    public class CSharpCyclomaticComplexityWalkerTests
    {
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
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var context = new ProcessContext();
            var walker = new CSharpCyclomaticComplexityWalker(context);
            dynamic file = new ExpandoObject();
            file.Identifier = "Foo.cs";

            walker.Visit(file, syntaxTree);

            Assert.True(context.ContainsKey("Foo.cs:Foo<TInput, TOutput>"));
            var type = context["Foo.cs:Foo<TInput, TOutput>"];
            Assert.Equal(1, type.CyclomaticComplexity);

            var function = context["Foo.cs:Foo<TInput, TOutput>:Bar()void"];
            Assert.Equal(1, function.CyclomaticComplexity);
        }
    }
}