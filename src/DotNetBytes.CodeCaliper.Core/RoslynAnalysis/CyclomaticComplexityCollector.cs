using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetBytes.CodeCaliper.Core.RoslynAnalysis
{
    using System.Threading.Tasks.Dataflow;

    public class CyclomaticComplexityCollector : TransformTask<SyntaxTreeLoader.Result, CyclomaticComplexityCollector.Result>
    {
        public CyclomaticComplexityCollector(ProcessContext context)
            : base(context, Environment.ProcessorCount * 2)
        {
        }

        public struct Result
        {
            public string FileId;

            public string[] TypeIds;

            public string[] FunctionIds;
        }

        protected override Result Transform(SyntaxTreeLoader.Result input)
        {
            throw new NotImplementedException();
        }
    }
}
