namespace DotNetBytes.CodeCaliper.Tests.Core
{
    using System.Linq;
    using DotNetBytes.CodeCaliper.Core;
    using Moq;
    using Xunit;

    public class FileEnumeratorTests
    {
        [Fact]
        public void Enumerate_WhenCreatedWithDefaultProperties_ThenEnumeratesAllFiles()
        {
            var expected = new[] {@"C:\Temp\foo.txt", @"C:\Temp\bar.txt"};
            var sourceMock = new Mock<FileEnumerator.ISource>();
            sourceMock.Setup(s => s.EnumerateFiles(@"C:\Temp")).Returns(expected);
            var fileEnumerator = new FileEnumerator(@"C:\Temp", sourceMock.Object);

            var actual = fileEnumerator.ToArray();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Enumerate_WhenExcludeRegexIsUsed_ThenEnumeratesOnlyThoseThatDoNotMatchTheRegex()
        {
            var source = new[] {@"C:\Temp\bar.cs", @"C:\Temp\baz.cs", @"C:\Temp\baz.Designer.cs"};
            var expected = new[] {@"C:\Temp\bar.cs", @"C:\Temp\baz.cs"};
            var sourceMock = new Mock<FileEnumerator.ISource>();
            sourceMock.Setup(s => s.EnumerateFiles(@"C:\Temp")).Returns(source);
            var fileEnumerator = new FileEnumerator(@"C:\Temp", sourceMock.Object) {Exclude = new[] {@"\.Designer\."}};

            var actual = fileEnumerator.ToArray();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Enumerate_WhenIncludeAndExcludeRegexAreCombined_ThenEnumeratesOnlyThoseThatMatchTheRegex()
        {
            var source = new[] {@"C:\Temp\foo.txt", @"C:\Temp\bar.cs", @"C:\Temp\baz.cs", @"C:\Temp\baz.Designer.cs"};
            var expected = new[] {@"C:\Temp\bar.cs", @"C:\Temp\baz.cs"};
            var sourceMock = new Mock<FileEnumerator.ISource>();
            sourceMock.Setup(s => s.EnumerateFiles(@"C:\Temp")).Returns(source);
            var fileEnumerator = new FileEnumerator(@"C:\Temp", sourceMock.Object)
            {
                Include = new[] {@"\.cs$"},
                Exclude = new[] {@"\.Designer\."}
            };

            var actual = fileEnumerator.ToArray();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Enumerate_WhenIncludeRegexIsUsed_ThenEnumeratesOnlyThoseThatMatchTheRegex()
        {
            var source = new[] {@"C:\Temp\foo.txt", @"C:\Temp\bar.cs", @"C:\Temp\baz.cs"};
            var expected = new[] {@"C:\Temp\bar.cs", @"C:\Temp\baz.cs"};
            var sourceMock = new Mock<FileEnumerator.ISource>();
            sourceMock.Setup(s => s.EnumerateFiles(@"C:\Temp")).Returns(source);
            var fileEnumerator = new FileEnumerator(@"C:\Temp", sourceMock.Object) {Include = new[] {@"\.cs$"}};

            var actual = fileEnumerator.ToArray();

            Assert.Equal(expected, actual);
        }
    }
}