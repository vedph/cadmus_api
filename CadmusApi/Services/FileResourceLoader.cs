using System;
using System.IO;
using System.Threading.Tasks;

namespace CadmusApi.Services
{
    /// <summary>
    /// File-system based resource loader.
    /// </summary>
    /// <seealso cref="CadmusApi.Services.IResourceLoader" />
    public sealed class FileResourceLoader : IResourceLoader
    {
        /// <summary>
        /// Loads the specified resource asynchronously.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        /// The resource stream, or null if file not found.
        /// </returns>
        /// <exception cref="ArgumentNullException">source</exception>
        public Task<Stream> LoadResourceAsync(string source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            if (!File.Exists(source)) return Task.FromResult((Stream)null);

            return Task.FromResult((Stream)new FileStream(
                source, FileMode.Open, FileAccess.Read, FileShare.Read));
        }
    }
}
