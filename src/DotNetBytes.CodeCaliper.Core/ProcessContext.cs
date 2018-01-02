namespace DotNetBytes.CodeCaliper.Core
{
    using System;
    using System.Collections.Concurrent;

    public class ProcessContext : ConcurrentDictionary<string, dynamic>
    {
        public ProcessContext()
            : base(Environment.ProcessorCount * 2, 1024)
        {
        }
    }
}