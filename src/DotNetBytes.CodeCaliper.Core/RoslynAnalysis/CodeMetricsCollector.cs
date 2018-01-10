namespace DotNetBytes.CodeCaliper.Core.RoslynAnalysis
{
    using System;
    using Microsoft.CodeAnalysis;

    public class CodeMetricsCollector : TransformTask<SyntaxTreeLoader.Result, CodeMetricsCollector.Result>
    {
        private readonly CSharpCodeMetricsWalker mCSharpWalker;

        public CodeMetricsCollector(ProcessContext context)
            : base(context, Environment.ProcessorCount * 2)
        {
            this.mCSharpWalker = new CSharpCodeMetricsWalker(context);
        }

        protected override Result Transform(SyntaxTreeLoader.Result input)
        {
            var file = this.Context[input.FileId];
            var syntaxTree = (SyntaxTree)file.SyntaxTree;
            return this.CollectMetrics(input.FileId, file, syntaxTree);
        }

        /// <summary>
        /// Collects the metrics for the supported file extensions.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="file">The file.</param>
        /// <param name="syntaxTree">The syntax tree.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public Result CollectMetrics(string fileId, dynamic file, SyntaxTree syntaxTree)
        {
            if (file.FullPath.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase))
            {
                this.mCSharpWalker.Visit(file, syntaxTree);
                return new Result { FileId = fileId };
            }

            throw new NotSupportedException();
        }

        public struct Result
        {
            public string FileId;
        }
    }
}