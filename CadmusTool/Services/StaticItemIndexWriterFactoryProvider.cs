using Cadmus.Index.Config;
using Cadmus.Index.Sql;
using Fusi.Microsoft.Extensions.Configuration.InMemoryJson;
using Microsoft.Extensions.Configuration;
using SimpleInjector;
using System;
using System.Reflection;

namespace CadmusTool.Services
{
    public sealed class StaticItemIndexWriterFactoryProvider :
        IItemIndexWriterFactoryProvider
    {
        private readonly string _connectionString;

        public StaticItemIndexWriterFactoryProvider(string connectionString)
        {
            _connectionString = connectionString ??
                throw new ArgumentNullException(nameof(connectionString));
        }

        public ItemIndexWriterFactory GetFactory(string profile)
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
            ItemIndexWriterFactory.ConfigureServices(
                container,
                indexAssemblies);

            container.Verify();

            // load seed configuration
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddInMemoryJson(profile);
            var configuration = builder.Build();

            return new ItemIndexWriterFactory(
                container,
                configuration,
                _connectionString);
        }
    }
}
