using Cadmus.Graph.Ef.PgSql;
using Cadmus.Index.Config;
using Cadmus.Index.Ef.PgSql;
using Fusi.Microsoft.Extensions.Configuration.InMemoryJson;
using Microsoft.Extensions.Hosting;
using System;

namespace Cadmus.Api.Services;

/// <summary>
/// Standard item index writer factory provider.
/// </summary>
/// <seealso cref="IItemIndexFactoryProvider" />
public sealed class StandardItemIndexFactoryProvider :
    IItemIndexFactoryProvider
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="StandardItemIndexFactoryProvider"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <exception cref="ArgumentNullException">connectionString</exception>
    public StandardItemIndexFactoryProvider(string connectionString)
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
                    // Cadmus.Index.Ef.MySql
                    typeof(EfPgSqlItemIndexWriter).Assembly);
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
    public ItemIndexFactory GetFactory(string profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return new ItemIndexFactory(GetHost(profile), _connectionString);
    }
}
