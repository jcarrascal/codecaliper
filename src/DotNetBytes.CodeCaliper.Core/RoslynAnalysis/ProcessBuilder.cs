namespace DotNetBytes.CodeCaliper.Core.RoslynAnalysis
{
    using System.Threading.Tasks.Dataflow;

    public static class ProcessBuilder
    {
        public static void Build(ProcessContext context, ISourceBlock<string> source)
        {
            var syntaxTreeLoader = new SyntaxTreeLoader(context);
            syntaxTreeLoader.LinkTo(source);
        }
    }
}