using Cadmus.Index.Config;
using Cadmus.Index.Sql;
using Fusi.Microsoft.Extensions.Configuration.InMemoryJson;
using Microsoft.Extensions.Configuration;
using SimpleInjector;
using System;
using System.Reflection;

namespace CadmusApi.Services
{
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

        /// <summary>
        /// Gets the part/fragment seeders factory.
        /// </summary>
        /// <param name="profile">The profile.</param>
        /// <returns>Factory.</returns>
        /// <exception cref="ArgumentNullException">profile</exception>
        public ItemIndexFactory GetFactory(string profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            // build the container for seeders
            Assembly[] indexAssemblies = new[]
            {
                // Cadmus.Index.Sql
                typeof(MySqlItemIndexWriter).Assembly
            };

            Container container = new Container();

            ItemIndexFactory.ConfigureServices(
                container,
                indexAssemblies);

            container.Verify();

            // load seed configuration
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddInMemoryJson(profile);
            var configuration = builder.Build();

            return new ItemIndexFactory(
                container,
                configuration,
                _connectionString);
        }
    }
}
