namespace DotNetBytes.CodeCaliper.Core.RoslynAnalysis
{
    using System;
    using System.Threading.Tasks.Dataflow;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.VisualBasic;
    using LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion;

    public class SyntaxTreeLoader
    {
        public SyntaxTreeLoader(ProcessContext context)
        {
            this.Context = context;
            this.BlockOptions =
                new ExecutionDataflowBlockOptions {MaxDegreeOfParallelism = Environment.ProcessorCount * 2};
            this.LinkOptions = new DataflowLinkOptions {PropagateCompletion = true};
            this.TransformBlock =
                new TransformBlock<string, Result>((Func<string, Result>) this.Transform, this.BlockOptions);
        }

        public ExecutionDataflowBlockOptions BlockOptions { get; }

        public DataflowLinkOptions LinkOptions { get; }

        public ProcessContext Context { get; }

        public TransformBlock<string, Result> TransformBlock { get; }

        public void LinkTo(ISourceBlock<string> source)
        {
            source.LinkTo(this.TransformBlock, this.LinkOptions, this.Predicate);
        }

        public bool Predicate(string fileId)
        {
            var fileInfo = this.Context[fileId];
            string fullPath = fileInfo.FullPath;
            return fullPath.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase) ||
                   fullPath.EndsWith(".vb", StringComparison.InvariantCultureIgnoreCase);
        }

        public Result Transform(string fileId)
        {
            var fileInfo = this.Context[fileId];
            string fullPath = fileInfo.FullPath;
            string sourceCode = fileInfo.SourceCode;
            fileInfo.SyntaxTree = this.LoadSyntaxTree(fullPath, sourceCode);
            return new Result {FileId = fileId};
        }

        public SyntaxTree LoadSyntaxTree(string fullPath, string sourceCode)
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