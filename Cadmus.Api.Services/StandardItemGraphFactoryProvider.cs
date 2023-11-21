using Cadmus.Graph.Ef.PgSql;
using Cadmus.Index.Config;
using Fusi.Microsoft.Extensions.Configuration.InMemoryJson;
using Microsoft.Extensions.Hosting;
using System;

namespace Cadmus.Api.Services;

/// <summary>
/// Standard item graph factory provider.
/// </summary>
/// <seealso cref="IItemGraphFactoryProvider" />
public sealed class StandardItemGraphFactoryProvider
    : IItemGraphFactoryProvider
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="StandardItemGraphFactoryProvider"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <exception cref="ArgumentNullException">connectionString</exception>
    public StandardItemGraphFactoryProvider(string connectionString)
    {
        _connectionString = connectionString ??
            throw new ArgumentNullException(nameof(connectionString));
    }

    private static IHost GetHost(string config)
    {
        return new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                ItemIndexFactory.ConfigureServices(services,
                    // Cadmus.Graph.Ef.MySql
                    typeof(EfPgSqlGraphRepository).Assembly);
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
    public ItemGraphFactory GetFactory(string profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return new ItemGraphFactory(GetHost(profile), _connectionString);
    }
}
