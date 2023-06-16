using System.IO;
using System.Threading.Tasks;

namespace Cadmus.Api.Services.Seeding;

/// <summary>
/// General purpose resource loader.
/// </summary>
public interface IResourceLoader
{
    /// <summary>
    /// Loads the specified resource asynchronously.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The resource stream.</returns>
    Task<Stream?> LoadResourceAsync(string source);
}
