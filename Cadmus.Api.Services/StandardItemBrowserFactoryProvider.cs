using Cadmus.Core.Config;
using Cadmus.Mongo;
using Fusi.Microsoft.Extensions.Configuration.InMemoryJson;
using Microsoft.Extensions.Configuration;
using SimpleInjector;
using System;
using System.Reflection;

namespace Cadmus.Api.Services
{
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

        /// <summary>
        /// Gets the item browsers factory.
        /// </summary>
        /// <param name="profile">The profile.</param>
        /// <returns>Factory.</returns>
        /// <exception cref="ArgumentNullException">profile</exception>
        public ItemBrowserFactory GetFactory(string profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            // build the tags to types map for parts/fragments
            Assembly[] browserAssemblies = new[]
            {
                // Cadmus.Mongo
                typeof(MongoHierarchyItemBrowser).Assembly,
            };
            TagAttributeToTypeMap map = new();
            map.Add(browserAssemblies);

            // build the container
            Container container = new();
            ItemBrowserFactory.ConfigureServices(
                container,
                new StandardPartTypeProvider(map),
                browserAssemblies);

            container.Verify();

            // load seed configuration
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddInMemoryJson(profile);
            var configuration = builder.Build();

            return new ItemBrowserFactory(
                container,
                configuration,
                _connectionString);
        }
    }
}
