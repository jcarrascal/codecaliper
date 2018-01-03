namespace DotNetBytes.CodeCaliper.Core
{
    using System;
    using System.Threading.Tasks.Dataflow;

    public abstract class TransformTask<TInput, TOutput>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransformTask{TInput, TOutput}" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="maxDegreeOfParallelism">The maximum degree of parallelism.</param>
        protected TransformTask(ProcessContext context, int maxDegreeOfParallelism)
        {
            this.Context = context;
            this.BlockOptions = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };
            this.LinkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            this.TransformBlock =
                new TransformBlock<TInput, TOutput>((Func<TInput, TOutput>)this.Transform, this.BlockOptions);
        }

        /// <summary>
        ///     Gets the transform block.
        /// </summary>
        /// <value>
        ///     The transform block.
        /// </value>
        public TransformBlock<TInput, TOutput> TransformBlock { get; }

        /// <summary>
        ///     Gets the link options.
        /// </summary>
        /// <value>
        ///     The link options.
        /// </value>
        public DataflowLinkOptions LinkOptions { get; }

        /// <summary>
        ///     Gets the context.
        /// </summary>
        /// <value>
        ///     The context.
        /// </value>
        public ProcessContext Context { get; }

        /// <summary>
        ///     Gets the block options.
        /// </summary>
        /// <value>
        ///     The block options.
        /// </value>
        public ExecutionDataflowBlockOptions BlockOptions { get; }

        /// <summary>
        ///     Transforms the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The corresponding output.</returns>
        protected abstract TOutput Transform(TInput input);

        /// <summary>
        ///     Predicate for filtering execution on the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns><c>true</c> if given the input it should execute; otherwise, <c>false</c>.</returns>
        public virtual bool Predicate(TInput input) => true;

        /// <summary>
        ///     Links this block to the specified source block.
        /// </summary>
        /// <param name="source">The source block.</param>
        public void LinkTo(ISourceBlock<TInput> source)
        {
            source.LinkTo(this.TransformBlock, this.LinkOptions, this.Predicate);
        }
    }
}