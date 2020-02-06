using Cadmus.Core;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using Cadmus.Mongo;
using Cadmus.Seed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CadmusApi.Services
{
    /// <summary>
    /// Database initializer extension to <see cref="IHost"/>.
    /// See https://stackoverflow.com/questions/45148389/how-to-seed-in-entity-framework-core-2.
    /// </summary>
    public static class HostSeedExtensions
    {
        /// <summary>
        /// Resolves directory variables in the specified path.
        /// Variables are defined in any part of the path between <c>%</c>.
        /// Currently, the variable <c>%wwwroot%</c> is reserved to resolve to
        /// the web content root directory; any other variable name is looked
        /// for in the configuration.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>Resolved path.</returns>
        private static string ResolvePath(string path,
            IServiceProvider serviceProvider)
        {
            if (path.IndexOf('%') == -1) return path;

            return new Regex("%([^%]+)%").Replace(path, (Match m) =>
            {
                switch (m.Groups[1].Value.ToUpperInvariant())
                {
                    case "WWWROOT":
                        IHostEnvironment env = serviceProvider
                            .GetService<IHostEnvironment>();
                        return Path.Combine(env.ContentRootPath, "wwwroot");
                    default:
                        IConfiguration config =
                            serviceProvider.GetService<IConfiguration>();
                        return config[m.Groups[1].Value];
                }
            });
        }

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

            // nope if database exists
            IDatabaseManager manager = new MongoDatabaseManager();
            if (manager.DatabaseExists(connString)) return;

            // load seed profile (nope if no profile)
            string profilePath = config["Seed:ProfilePath"];
            if (string.IsNullOrEmpty(profilePath)) return;

            profilePath = ResolvePath(profilePath, serviceProvider);
            if (!File.Exists(profilePath)) return;

            Console.WriteLine($"Loading seed profile from {profilePath}...");
            logger.LogInformation($"Loading seed profile from {profilePath}...");

            string profileContent;
            using (StreamReader reader = new StreamReader(
                new FileStream(profilePath, FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite), Encoding.UTF8))
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
            logger.LogInformation("Creating database {connString}...");

            manager.CreateDatabase(connString, profile);

            Console.WriteLine("Database created.");
            logger.LogInformation("Database created.");

            // get seed count
            int count = 0;
            string configCount = config["Seed:ItemCount"];
            if (configCount != null && int.TryParse(configCount, out int n))
                count = n;

            // if required, seed data
            if (count > 0)
            {
                Console.WriteLine("Creating repository...");
                Serilog.Log.Information("Creating repository...");

                IRepositoryProvider repositoryProvider =
                    serviceProvider.GetService<IRepositoryProvider>();
                ICadmusRepository repository =
                    repositoryProvider.CreateRepository(databaseName);

                Console.WriteLine("Seeding items...");
                IPartSeederFactoryProvider seederService =
                    serviceProvider.GetService<IPartSeederFactoryProvider>();

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
