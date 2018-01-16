using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetBytes.CodeCaliper.Core
{
    using System.Threading.Tasks.Dataflow;

    public class CodeMetricsFormatter
    {
        public CodeMetricsFormatter(ProcessContext context)
        {
            this.Context = context;
            this.BlockOptions = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 };
            this.LinkOptions = new DataflowLinkOptions { PropagateCompletion = true };
        }

        public ProcessContext Context { get; }
        public ExecutionDataflowBlockOptions BlockOptions { get; }
        public DataflowLinkOptions LinkOptions { get; }
    }
}
