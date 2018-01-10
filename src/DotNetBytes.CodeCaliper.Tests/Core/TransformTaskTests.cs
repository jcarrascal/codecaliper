namespace DotNetBytes.CodeCaliper.Tests.Core
{
    using DotNetBytes.CodeCaliper.Core;
    using Xunit;

    public class TransformTaskTests
    {
        internal class TestTransformClass : TransformTask<int, string>
        {
            public TestTransformClass(ProcessContext context, int maxDegreeOfParallelism)
                : base(context, maxDegreeOfParallelism)
            {
            }

            protected override string Transform(int input)
            {
                return input.ToString();
            }
        }

        [Fact]
        public void Ctor_WhenGivenTheProcessContextAndDegreeOfParallelism_ThenTheBlockIsSetupAccordingly()
        {
            var processContext = new ProcessContext();

            var transformClass = new TestTransformClass(processContext, 10);

            Assert.Equal(processContext, transformClass.Context);
            Assert.NotNull(transformClass.BlockOptions);
            Assert.Equal(10, transformClass.BlockOptions.MaxDegreeOfParallelism);
            Assert.NotNull(transformClass.LinkOptions);
            Assert.True(transformClass.LinkOptions.PropagateCompletion);
            Assert.NotNull(transformClass.TransformBlock);
        }
    }
}