using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetBytes.CodeCaliper.Tests.Core
{
    using DotNetBytes.CodeCaliper.Core;
    using DotNetBytes.CodeCaliper.Core.RoslynAnalysis;
    using Xunit;

    public class TransformTaskTests
    {
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
    }
}
