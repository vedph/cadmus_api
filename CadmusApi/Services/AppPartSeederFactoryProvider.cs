using Cadmus.Core.Config;
using Cadmus.Img.Parts;
using Cadmus.Seed;
using Cadmus.Seed.General.Parts;
using Cadmus.Seed.Img.Parts;
using Cadmus.Seed.Philology.Parts;
using Fusi.Microsoft.Extensions.Configuration.InMemoryJson;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;

namespace CadmusApi.Services;

/// <summary>
/// Application's part seeders factory provider.
/// </summary>
public sealed class AppPartSeederFactoryProvider : IPartSeederFactoryProvider
{
    private static IHost GetHost(string config)
    {
        // build the tags to types map for parts/fragments
        Assembly[] seedAssemblies = new[]
        {
            // Cadmus.General.Seed.Parts
            typeof(NotePartSeeder).Assembly,
            // Cadmus.Seed.Philology.Parts
            typeof(ApparatusLayerFragmentSeeder).Assembly,
            // Cadmus.Seed.Img.Parts
            typeof(GalleryImageAnnotationsPartSeeder).GetTypeInfo().Assembly
        };
        TagAttributeToTypeMap map = new();
        map.Add(seedAssemblies);

        return new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                PartSeederFactory.ConfigureServices(services,
                    new StandardPartTypeProvider(map),
                    seedAssemblies);
            })
            // extension method from Fusi library
            .AddInMemoryJson(config)
            .Build();
    }

    /// <summary>
    /// Gets the part/fragment seeders factory.
    /// </summary>
    /// <param name="profile">The profile.</param>
    /// <returns>Factory.</returns>
    /// <exception cref="ArgumentNullException">profile</exception>
    public PartSeederFactory GetFactory(string profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return new PartSeederFactory(GetHost(profile));
    }
}
