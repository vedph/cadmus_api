using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cadmus.Api.Services.Seeding;

/// <summary>
/// Service used to load resources via <see cref="IResourceLoader"/>'s.
/// This just selects the correct provider according to the source,
/// and eventually resolves variables in a file-based path before loading.
/// Currently, the only loaders are file-system based and HTTP-based,
/// so we just check if the source starts with <c>http</c>.
/// </summary>
/// <remarks>This service is eventually used at startup when importing data
/// is required and the database did not exist, by
/// <see cref="HostSeedExtensions"/>.</remarks>
public class ResourceLoaderService
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceLoaderService"/>
    /// class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <exception cref="ArgumentNullException">serviceProvider</exception>
    public ResourceLoaderService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider
            ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Resolves directory variables in the specified path.
    /// Variables are defined in any part of the path between <c>%</c>.
    /// Currently, the variable <c>%wwwroot%</c> is reserved to resolve to
    /// the web content root directory; any other variable name is looked
    /// for in the configuration.
    /// </summary>
    /// <exception cref="ArgumentNullException">path</exception>
    /// <param name="path">The path.</param>
    /// <returns>Resolved path.</returns>
    public string ResolvePath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (!path.Contains('%')) return path;

        return new Regex("%([^%]+)%").Replace(path, (Match m) =>
        {
            switch (m.Groups[1].Value.ToUpperInvariant())
            {
                case "WWWROOT":
                    IHostEnvironment env =
                        _serviceProvider.GetService<IHostEnvironment>()!;
                    return Path.Combine(env.ContentRootPath, "wwwroot");

                default:
                    IConfiguration config =
                        _serviceProvider.GetService<IConfiguration>()!;
                    return config[m.Groups[1].Value]!;
            }
        });
    }

    /// <summary>
    /// Gets the resolved source from the specified source.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The resolved load path.</returns>
    /// <exception cref="ArgumentNullException">source</exception>
    public string ResolveSource(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return source;

        return ResolvePath(source);
    }

    /// <summary>
    /// Gets the loader for the specified source.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The loader.</returns>
    /// <exception cref="ArgumentNullException">source</exception>
    public async Task<Stream?> LoadAsync(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        string resolved = ResolveSource(source);

        return resolved.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? await new HttpResourceLoader().LoadResourceAsync(resolved)
            : await new FileResourceLoader().LoadResourceAsync(resolved);
    }
}
