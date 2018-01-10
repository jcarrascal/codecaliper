namespace DotNetBytes.CodeCaliper.Tests.Core.RoslynAnalysis
{
    using System.Dynamic;
    using DotNetBytes.CodeCaliper.Core;
    using DotNetBytes.CodeCaliper.Core.RoslynAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Xunit;

    public class CodeMetricsCollectorTests
    {
        [Fact]
        public void Transform_WhenGivenAnUnsupportedFile_ThenThrowsNotSupportedException()
        {
            var fileId = "Foo.cs";
            dynamic file = new ExpandoObject();
            file.FullPath = "Foo.cs";
            var sourceCode = string.Empty;
            var options = new CSharpParseOptions(LanguageVersion.Latest);
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode, options, "Foo.cs");

            var codeMetricsCollector = new CodeMetricsCollector(new ProcessContext());
            CodeMetricsCollector.Result result = codeMetricsCollector.CollectMetrics(fileId, file, syntaxTree);

            Assert.Equal(fileId, result.FileId);
            Assert.Equal(0, file.CyclomaticComplexity);
            Assert.Equal(0, file.SourceLinesOfCode);
        }
    }
}