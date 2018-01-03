namespace DotNetBytes.CodeCaliper.Core.RoslynAnalysis
{
    using System;
    using System.Threading.Tasks.Dataflow;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.VisualBasic;
    using LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion;

    public class SyntaxTreeLoader : TransformTask<string, SyntaxTreeLoader.Result>
    {
        public SyntaxTreeLoader(ProcessContext context)
            : base(context, Environment.ProcessorCount * 2)
        {
        }

        public override bool Predicate(string fileId)
        {
            var fileInfo = this.Context[fileId];
            string fullPath = fileInfo.FullPath;
            return fullPath.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase) ||
                   fullPath.EndsWith(".vb", StringComparison.InvariantCultureIgnoreCase);
        }

        protected override Result Transform(string fileId)
        {
            var fileInfo = this.Context[fileId];
            string fullPath = fileInfo.FullPath;
            string sourceCode = fileInfo.SourceCode;
            fileInfo.SyntaxTree = this.LoadSyntaxTree(fullPath, sourceCode);
            return new Result {FileId = fileId};
        }

        internal SyntaxTree LoadSyntaxTree(string fullPath, string sourceCode)
        {
            if (fullPath.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase))
            {
                var options = new CSharpParseOptions(LanguageVersion.Latest);
                return CSharpSyntaxTree.ParseText(sourceCode, options, fullPath);
            }

            if (fullPath.EndsWith(".vb", StringComparison.InvariantCultureIgnoreCase))
            {
                var options = new VisualBasicParseOptions(Microsoft.CodeAnalysis.VisualBasic.LanguageVersion.Latest);
                return VisualBasicSyntaxTree.ParseText(sourceCode, options, fullPath);
            }

            throw new NotSupportedException();
        }

        public struct Result
        {
            public string FileId;
        }
    }
}