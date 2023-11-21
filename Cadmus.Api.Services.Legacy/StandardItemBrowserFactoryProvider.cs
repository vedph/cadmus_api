using Cadmus.Core.Config;
using Cadmus.Mongo;
using Fusi.Microsoft.Extensions.Configuration.InMemoryJson;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;

namespace Cadmus.Api.Services;

/// <summary>
/// Standard items browser factory provider.
/// </summary>
/// <seealso cref="IItemBrowserFactoryProvider" />
public sealed class StandardItemBrowserFactoryProvider :
    IItemBrowserFactoryProvider
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="StandardItemBrowserFactoryProvider"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    public StandardItemBrowserFactoryProvider(string connectionString)
    {
        _connectionString = connectionString ??
            throw new ArgumentNullException(nameof(connectionString));
    }

    private static IHost GetHost(string config)
    {
        // build the tags to types map for parts/fragments
        Assembly[] browserAssemblies = new[]
        {
            // Cadmus.Mongo
            typeof(MongoHierarchyItemBrowser).Assembly,
        };
        TagAttributeToTypeMap map = new();
        map.Add(browserAssemblies);

        return new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                ItemBrowserFactory.ConfigureServices(services,
                    new StandardPartTypeProvider(map),
                    browserAssemblies);
            })
            // extension method from Fusi library
            .AddInMemoryJson(config)
            .Build();
    }

    /// <summary>
    /// Gets the item browsers factory.
    /// </summary>
    /// <param name="profile">The profile.</param>
    /// <returns>Factory.</returns>
    /// <exception cref="ArgumentNullException">profile</exception>
    public ItemBrowserFactory GetFactory(string profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return new ItemBrowserFactory(GetHost(profile), _connectionString);
    }
}
