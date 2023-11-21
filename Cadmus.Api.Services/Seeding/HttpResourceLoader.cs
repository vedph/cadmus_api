using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Cadmus.Api.Services.Seeding;

/// <summary>
/// HTTP-based resource loader.
/// </summary>
public sealed class HttpResourceLoader : IResourceLoader
{
    /// <summary>
    /// Loads the specified resource asynchronously.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>
    /// The resource stream, or null in case of error.
    /// </returns>
    /// <exception cref="ArgumentNullException">source</exception>
    public async Task<Stream?> LoadResourceAsync(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        Stream? stream;
        try
        {
            HttpClient client = new();
            stream = await client.GetStreamAsync(source);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            stream = null;
        }
        return stream;
    }
}
