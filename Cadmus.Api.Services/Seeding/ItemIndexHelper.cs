using Cadmus.Index.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Cadmus.Api.Services.Seeding;

/// <summary>
/// Item index/graph helper.
/// </summary>
public static class ItemIndexHelper
{
    /// <summary>
    /// Loads the Cadmus profile from the source specified in Seed:ProfileSource,
    /// or from the default location if this is not set.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>Profile, or null if not found.</returns>
    /// <exception cref="InvalidOperationException">profile file not found
    /// </exception>
    private async static Task<string?> LoadProfileAsync(
        IConfiguration configuration, IServiceProvider serviceProvider)
    {
        string? profileSource = configuration["Seed:ProfileSource"]
            ?? "%wwwroot%/seed-profile.json";

        ResourceLoaderService loaderService = new(serviceProvider);

        string profile;
        using Stream? stream = await loaderService.LoadAsync(profileSource) ??
            throw new InvalidOperationException(
                $"Profile file expected at {profileSource} not found");
        using (StreamReader reader = new(stream, Encoding.UTF8))
        {
            profile = reader.ReadToEnd();
        }
        return profile;
    }

    /// <summary>
    /// Gets the index factory.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>Factory or null.</returns>
    public static async Task<ItemIndexFactory?> GetIndexFactoryAsync(
        IConfiguration configuration, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        string? profile = await LoadProfileAsync(configuration, serviceProvider);
        if (profile == null) return null;

        IItemIndexFactoryProvider factoryProvider =
            serviceProvider.GetService<IItemIndexFactoryProvider>()!;
        return factoryProvider.GetFactory(profile);
    }

    /// <summary>
    /// Gets the graph factory.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>Factory or null.</returns>
    public static async Task<ItemGraphFactory?> GetGraphFactoryAsync(
        IConfiguration configuration, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        string? profile = await LoadProfileAsync(configuration, serviceProvider);
        if (profile == null) return null;

        IItemGraphFactoryProvider factoryProvider =
            serviceProvider.GetService<IItemGraphFactoryProvider>()!;
        return factoryProvider.GetFactory(profile);
    }
}
