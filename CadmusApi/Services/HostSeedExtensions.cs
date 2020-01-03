using Cadmus.Core;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using Cadmus.Mongo;
using Cadmus.Seed;
using CadmusApi.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CadmusApi.Services
{
    /// <summary>
    /// Database initializer extension to <see cref="IHost"/>.
    /// See https://stackoverflow.com/questions/45148389/how-to-seed-in-entity-framework-core-2.
    /// </summary>
    public static class HostSeedExtensions
    {
        private static void SeedCadmusDatabase(
            IServiceProvider serviceProvider,
            IConfiguration config,
            ILogger logger)
        {
            // build connection string
            string connString = config.GetConnectionString("Default");
            string databaseName = config["DatabaseNames:Data"];
            if (string.IsNullOrEmpty(databaseName)) return;
            connString = string.Format(connString, databaseName);

            // load seed profile
            string profilePath = config["Seed:ProfilePath"];
            if (string.IsNullOrEmpty(profilePath) || !File.Exists(profilePath))
                return;

            Console.WriteLine($"Loading seed profile from {profilePath}...");
            logger.LogInformation($"Loading seed profile from {profilePath}...");

            IDatabaseManager manager = new MongoDatabaseManager();
            string profileContent;
            using (StreamReader reader = new StreamReader(
                new FileStream(profilePath, FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite), Encoding.UTF8))
            {
                profileContent = reader.ReadToEnd();
            }

            IDataProfileSerializer serializer = new JsonDataProfileSerializer();
            DataProfile profile = serializer.Read(profileContent);

            // create database if not exists
            bool seed = false;
            if (!manager.DatabaseExists(connString))
            {
                Console.WriteLine("Creating database...");
                logger.LogInformation("Creating database {connString}...");

                manager.CreateDatabase(connString, profile);

                Console.WriteLine("Database created.");
                logger.LogInformation("Database created.");
                seed = true;
            }

            // create repository
            Console.WriteLine("Creating repository...");
            Serilog.Log.Information("Creating repository...");

            IRepositoryService repositoryService =
                serviceProvider.GetService<IRepositoryService>();
            ICadmusRepository repository =
                repositoryService.CreateRepository(databaseName);

            // seed items if database was created and item(s) are requested
            int count = 0;
            string configCount = config["Seed:ItemCount"];
            if (configCount != null && int.TryParse(configCount, out int n))
                count = n;

            if (seed && count > 0)
            {
                Console.WriteLine("Seeding items...");
                ISeederService seederService = serviceProvider.GetService<ISeederService>();

                PartSeederFactory factory = seederService.GetFactory(profilePath);
                CadmusSeeder seeder = new CadmusSeeder(factory);
                foreach (IItem item in seeder.GetItems(count))
                {
                    Console.WriteLine($"{item}: {item.Parts.Count} parts");
                    repository.AddItem(item, true);
                    foreach (IPart part in item.Parts)
                    {
                        repository.AddPart(part, true);
                    }
                }
                Console.WriteLine("Seeding completed.");
            }
        }

        /// <summary>
        /// Seeds the database.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <returns>The received host, to allow concatenation.</returns>
        /// <exception cref="ArgumentNullException">serviceProvider</exception>
        public static IHost Seed(this IHost host)
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

                    Task.Run(async () =>
                    {
                        // seed accounts database
                        ApplicationDatabaseInitializer initializer =
                            new ApplicationDatabaseInitializer(serviceProvider);

                        await Policy.Handle<DbException>()
                            .WaitAndRetry(new[]
                            {
                                TimeSpan.FromSeconds(10),
                                TimeSpan.FromSeconds(30),
                                TimeSpan.FromSeconds(60)
                            }, (exception, timeSpan, _) =>
                            {
                                string message = "Unable to connect to DB" +
                                    $" (sleep {timeSpan}): {exception.Message}";
                                Console.WriteLine(message);
                                logger.LogError(exception, message);
                            }).Execute(async () =>
                            {
                                await initializer.SeedAsync();
                            });
                    }).Wait();

                    // seed Cadmus database
                    Task.Run(async () =>
                    {
                        await Policy.Handle<DbException>()
                            .WaitAndRetry(new[]
                            {
                                TimeSpan.FromSeconds(10),
                                TimeSpan.FromSeconds(30),
                                TimeSpan.FromSeconds(60)
                            }, (exception, timeSpan, _) =>
                            {
                                string message = "Unable to connect to DB" +
                                    $" (sleep {timeSpan}): {exception.Message}";
                                Console.WriteLine(message);
                                logger.LogError(exception, message);
                            }).Execute(() =>
                            {
                                SeedCadmusDatabase(serviceProvider, config, logger);
                                return Task.CompletedTask;
                            });
                    }).Wait();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                    throw;
                }
            }
            return host;
        }
    }
}
