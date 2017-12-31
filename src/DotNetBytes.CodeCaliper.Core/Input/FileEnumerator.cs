namespace DotNetBytes.CodeCaliper.Core.Input
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    ///     Enumerates files on a base path that match a series of regular expressions.
    /// </summary>
    /// <seealso cref="string" />
    public class FileEnumerator : IEnumerable<string>
    {
        private readonly string mPath;
        private readonly ISource mSource;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FileEnumerator" /> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public FileEnumerator(string path)
            : this(path, new DefaultSource())
        {
        }

        internal FileEnumerator(string path, ISource source)
        {
            this.mPath = path;
            this.mSource = source;
        }

        /// <summary>
        ///     Gets or sets the include patterns.
        /// </summary>
        /// <value>
        ///     The include patterns.
        /// </value>
        public string[] Include { get; set; }

        /// <summary>
        ///     Gets or sets the exclude patterns.
        /// </summary>
        /// <value>
        ///     The exclude patterns.
        /// </value>
        public string[] Exclude { get; set; }

        /// <inheritdoc />
        public IEnumerator<string> GetEnumerator()
        {
            var include = this.Compile(this.Include, true);
            var exclude = this.Compile(this.Exclude, false);
            foreach (var filename in this.mSource.EnumerateFiles(this.mPath))
                if (include(filename) && !exclude(filename))
                    yield return filename;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private Func<string, bool> Compile(string[] patterns, bool defaultResult)
        {
            if (patterns != null && patterns.Length > 0)
            {
                var regex = patterns.Select(p => new Regex(p, RegexOptions.Compiled | RegexOptions.IgnoreCase))
                    .ToArray();
                return input => regex.Any(r => r.IsMatch(input));
            }

            return _ => defaultResult;
        }

        /// <summary>
        ///     This interface exists mostly for facilitating unit testing.
        /// </summary>
        public interface ISource
        {
            /// <summary>
            ///     Enumerates the files of the specified path.
            /// </summary>
            /// <param name="path">The path.</param>
            /// <returns></returns>
            IEnumerable<string> EnumerateFiles(string path);
        }

        /// <summary>
        ///     Default implementation used at runtime.
        /// </summary>
        /// <seealso cref="DotNetBytes.CodeCaliper.Core.Input.FileEnumerator.ISource" />
        public class DefaultSource : ISource
        {
            /// <inheritdoc />
            public IEnumerable<string> EnumerateFiles(string path)
            {
                return Directory.EnumerateFiles(path, string.Empty, SearchOption.AllDirectories);
            }
        }
    }
}