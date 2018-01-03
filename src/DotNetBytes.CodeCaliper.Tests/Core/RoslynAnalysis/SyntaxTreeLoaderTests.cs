namespace DotNetBytes.CodeCaliper.Tests.Core.RoslynAnalysis
{
    using System.Collections.Generic;
    using System.Dynamic;
    using DotNetBytes.CodeCaliper.Core;
    using DotNetBytes.CodeCaliper.Core.RoslynAnalysis;
    using Xunit;

    public class SyntaxTreeLoaderTests
    {
        public static dynamic CreateFile(string fullPath, string sourceCode = "")
        {
            dynamic file = new ExpandoObject();
            file.FullPath = fullPath;
            file.SourceCode = sourceCode;
            return file;
        }

        [Fact]
        public void LoadSyntaxTree_WhenGivenTheFullPathAndCSharpCode_ThenReturnsTheSyntaxTree()
        {
            var processContext = new ProcessContext();
            var syntaxTreeLoader = new SyntaxTreeLoader(processContext);

            var syntaxTree = syntaxTreeLoader.LoadSyntaxTree(@"C:\Temp\foo.CS", "class Program {}");

            Assert.NotNull(syntaxTree);
            Assert.Equal("class Program {}", syntaxTree.ToString());
        }

        [Fact]
        public void LoadSyntaxTree_WhenGivenTheFullPathAndVisualBasicCode_ThenReturnsTheSyntaxTree()
        {
            var processContext = new ProcessContext();
            var syntaxTreeLoader = new SyntaxTreeLoader(processContext);

            var syntaxTree = syntaxTreeLoader.LoadSyntaxTree(@"C:\Temp\foo.Vb", "Public Module mymod\r\nEnd Module");

            Assert.NotNull(syntaxTree);
            Assert.Equal("Public Module mymod\r\nEnd Module", syntaxTree.ToString());
        }

        [Fact]
        public void Predicate_WhenGivenAFileIdInTheContext_ThenOnlyAcceptsCShparpAndVisualBasicExtensions()
        {
            var processContext = new ProcessContext
            {
                ["foo.cs"] = CreateFile(@"C:\Temp\foo.cs"),
                ["foo.vb"] = CreateFile(@"C:\Temp\foo.vb"),
                ["foo.db"] = CreateFile(@"C:\Temp\foo.db")
            };

            var syntaxTreeLoader = new SyntaxTreeLoader(processContext);

            Assert.True(syntaxTreeLoader.Predicate("foo.cs"));
            Assert.True(syntaxTreeLoader.Predicate("foo.vb"));
            Assert.False(syntaxTreeLoader.Predicate("foo.db"));
        }

        [Fact]
        public void Predicate_WhenGivenAFileIdThatIsNotInTheContext_ThenThrowsAnKeyNotFoundException()
        {
            var processContext = new ProcessContext();

            var syntaxTreeLoader = new SyntaxTreeLoader(processContext);

            Assert.Throws<KeyNotFoundException>(() => syntaxTreeLoader.Predicate("foo.cs"));
        }
    }
}