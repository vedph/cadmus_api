using Cadmus.Core;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using Cadmus.Index;
using Cadmus.Index.Config;
using Cadmus.Index.Graph;
using Cadmus.Mongo;
using Cadmus.Seed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cadmus.Api.Services.Seeding
{
    /// <summary>
    /// Database initializer extension to <see cref="IHost"/>.
    /// See https://stackoverflow.com/questions/45148389/how-to-seed-in-entity-framework-core-2.
    /// </summary>
    public static class HostSeedExtensions
    {
        private static void UpdateGraph(IItem item, IGraphRepository repository,
            ILogger logger)
        {
            NodeMapper mapper = new NodeMapper(repository)
            {
                Logger = logger
            };
            GraphSet set = mapper.MapItem(item);

            logger.LogInformation("Updating graph " + set);
            repository.UpdateGraph(set);
        }

        private static async Task SeedItemsAsync(
            IServiceProvider serviceProvider, IConfiguration config,
            PartSeederFactory factory, ICadmusRepository repository,
            int count, string graphSql)
        {
            ILogger logger = serviceProvider
                .GetService<ILoggerFactory>()
                .CreateLogger(typeof(HostSeedExtensions));

            // determine if graph is enabled, and eventually get the graph repository
            bool isGraphEnabled = config.GetValue<bool>("Indexing:IsGraphEnabled");
            // when graph is enabled, index is automatically enabled
            bool isIndexEnabled = isGraphEnabled ||
                config.GetValue<bool>("Indexing:IsEnabled");

            IGraphRepository graphRepository = null;
            IItemIndexWriter indexWriter = null;

            // get graph and index services
            if (isGraphEnabled)
            {
                graphRepository = serviceProvider.GetService<IGraphRepository>();
                if (graphRepository == null)
                    logger?.LogError("Unable to get IGraphRepository service");
            }
            if (isIndexEnabled)
            {
                Console.WriteLine("Getting index writer...");
                ItemIndexFactory indexFactory = await ItemIndexHelper
                    .GetIndexFactoryAsync(config, serviceProvider);
                indexWriter = indexFactory.GetItemIndexWriter(isGraphEnabled,
                    graphSql);
            }

            // wait before indexing if requested
            if (isIndexEnabled)
            {
                // delay if requested, to allow DB start
                int delay = config.GetValue<int>("Seed:IndexDelay");
                if (delay > 0)
                {
                    Console.WriteLine($"Waiting for {delay} seconds...");
                    Thread.Sleep(delay * 1000);
                }
            }

            // seed items with their parts
            Console.WriteLine($"Seeding {count} items...");
            CadmusSeeder seeder = new CadmusSeeder(factory);

            foreach (IItem item in seeder.GetItems(count))
            {
                Console.WriteLine($"{item}: {item.Parts.Count} parts");
                repository.AddItem(item, true);
                foreach (IPart part in item.Parts)
                {
                    repository.AddPart(part, true);
                }

                // write the item's index data if requested
                if (indexWriter != null)
                {
                    Console.WriteLine($"Adding to index {item.Id}");
                    await indexWriter.WriteItem(item);

                    // update also the graph if enabled
                    if (isGraphEnabled)
                        UpdateGraph(item, graphRepository, logger);
                }
            }
            indexWriter?.Close();
            Console.WriteLine("Seeding completed.");
        }

        private static async Task SeedCadmusDatabaseAsync(
            IServiceProvider serviceProvider,
            IConfiguration config,
            string graphSql,
            ILogger logger)
        {
            // build connection string
            string connString = config.GetConnectionString("Default");
            string databaseName = config["DatabaseNames:Data"];
            if (string.IsNullOrEmpty(databaseName))
            {
                Console.WriteLine("No database name in config DatabaseNames:Data");
                return;
            }
            connString = string.Format(connString, databaseName);
            Console.WriteLine($"Database: {databaseName}");

            // nope if database exists
            IDatabaseManager manager = new MongoDatabaseManager();
            if (manager.DatabaseExists(connString))
            {
                Console.WriteLine($"Database {databaseName} already exists");
                return;
            }

            // load seed profile (nope if no profile)
            string profileSource = config["Seed:ProfileSource"];
            if (string.IsNullOrEmpty(profileSource))
            {
                Console.WriteLine("No profile source in config Seed:ProfileSource");
                return;
            }

            Console.WriteLine($"Loading seed profile from {profileSource}...");
            logger.LogInformation($"Loading seed profile from {profileSource}...");

            ResourceLoaderService loaderService =
                new ResourceLoaderService(serviceProvider);
            Console.WriteLine($"Source: {loaderService.ResolveSource(profileSource)}");

            Stream stream = await loaderService.LoadAsync(profileSource);
            if (stream == null)
            {
                Console.WriteLine("Error: seed profile could not be loaded");
                return;
            }

            // deserialize the profile
            Console.WriteLine("Loading profile...");
            string profileContent;
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                profileContent = reader.ReadToEnd();
            }

            IDataProfileSerializer serializer = new JsonDataProfileSerializer();
            DataProfile profile = serializer.Read(profileContent);

            // issue warning on invalid profile
            Console.WriteLine("Validating profile...");
            string error = profile.Validate();
            if (error != null) logger.LogWarning(error);

            // create database
            Console.WriteLine("Creating database...");
            logger.LogInformation($"Creating database {connString}...");

            manager.CreateDatabase(connString, profile);

            Console.WriteLine("Database created.");
            logger.LogInformation("Database created.");

            // get the required services
            Console.WriteLine("Creating seeders factory...");
            Serilog.Log.Information("Creating seeders factory...");
            IPartSeederFactoryProvider factoryProvider =
                serviceProvider.GetService<IPartSeederFactoryProvider>();
            PartSeederFactory factory;
            using (StreamReader reader = new StreamReader(
                await loaderService.LoadAsync(profileSource),
                Encoding.UTF8))
            {
                factory = factoryProvider.GetFactory(reader.ReadToEnd());
            }

            Console.WriteLine("Creating repository...");
            Serilog.Log.Information("Creating repository...");
            IRepositoryProvider repositoryProvider =
                serviceProvider.GetService<IRepositoryProvider>();
            ICadmusRepository repository =
                repositoryProvider.CreateRepository();

            // get seed count
            int count = 0;
            string configCount = config["Seed:ItemCount"];
            if (configCount != null && int.TryParse(configCount, out int n))
                count = n;

            // if required, seed data
            if (count > 0)
            {
                await SeedItemsAsync(serviceProvider, config, factory,
                    repository, count, graphSql);
            }

            // import data if required
            IList<string> sources = factory.GetImports();
            if (sources?.Count > 0)
            {
                PartImporter importer = new PartImporter(repository);

                foreach (string source in sources)
                {
                    foreach (string resolved in SourceRangeResolver.Resolve(source))
                    {
                        Console.WriteLine($"Importing from {resolved}...");
                        using Stream jsonStream =
                            await loaderService.LoadAsync(resolved);
                        importer.Import(jsonStream);
                    }
                }
            }
        }

        private static Task SeedAccountsAsync(IServiceProvider serviceProvider)
        {
            ApplicationDatabaseInitializer initializer =
                new ApplicationDatabaseInitializer(serviceProvider);

            return Policy.Handle<DbException>()
                .WaitAndRetry(new[]
                {
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromSeconds(60)
                }, (exception, timeSpan, _) =>
                {
                    ILogger logger = serviceProvider
                        .GetService<ILoggerFactory>()
                        .CreateLogger(typeof(HostSeedExtensions));

                    string message = "Unable to connect to DB" +
                        $" (sleep {timeSpan}): {exception.Message}";
                    Console.WriteLine(message);
                    logger.LogError(exception, message);
                }).Execute(async () =>
                {
                    await initializer.SeedAsync();
                });
        }

        private static Task SeedCadmusAsync(IServiceProvider serviceProvider,
            string graphSql)
        {
            return Policy.Handle<DbException>()
                .WaitAndRetry(new[]
                {
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromSeconds(60)
                }, (exception, timeSpan, _) =>
                {
                    ILogger logger = serviceProvider
                        .GetService<ILoggerFactory>()
                        .CreateLogger(typeof(HostSeedExtensions));

                    string message = "Unable to connect to DB" +
                        $" (sleep {timeSpan}): {exception.Message}";
                    Console.WriteLine(message);
                    logger.LogError(exception, message);
                }).Execute(async () =>
                {
                    IConfiguration config =
                        serviceProvider.GetService<IConfiguration>();

                    ILogger logger = serviceProvider
                        .GetService<ILoggerFactory>()
                        .CreateLogger(typeof(HostSeedExtensions));

                    Console.WriteLine("Seeding database...");
                    await SeedCadmusDatabaseAsync(
                        serviceProvider,
                        config,
                        graphSql,
                        logger);
                    Console.WriteLine("Seeding completed");
                    return Task.CompletedTask;
                });
        }

        /// <summary>
        /// Seeds the database.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="accounts">True to seed the accounts.</param>
        /// <param name="data">True to seed the data.</param>
        /// <param name="graphSql">The optional SQL code to seed the index
        /// for using it with the graph. This is usually provided by
        /// project-specific implementations.</param>
        /// <returns>The received host, to allow concatenation.</returns>
        /// <exception cref="ArgumentNullException">serviceProvider</exception>
        public static async Task<IHost> SeedAsync(this IHost host,
            bool accounts = true, bool data = true, string graphSql = null)
        {
            using (var scope = host.Services.CreateScope())
            {
                IServiceProvider serviceProvider = scope.ServiceProvider;
                ILogger logger = serviceProvider
                    .GetService<ILoggerFactory>()
                    .CreateLogger(typeof(HostSeedExtensions));

                try
                {
                    IConfiguration config =
                        serviceProvider.GetService<IConfiguration>();

                    if (accounts)
                        await SeedAccountsAsync(serviceProvider);

                    if (data)
                        await SeedCadmusAsync(serviceProvider, graphSql);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    logger.LogError(ex, ex.Message);
                    throw;
                }
            }
            return host;
        }
    }
}
