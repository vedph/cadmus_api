using Cadmus.Core;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using Cadmus.Mongo;
using Cadmus.Seed;
using CadmusTool.Services;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace CadmusTool.Commands
{
    public sealed class SeedDatabaseCommand : ICommand
    {
        private readonly IConfiguration _config;
        private readonly RepositoryService _repositoryService;
        private readonly SeederService _seederService;
        private readonly string _database;
        private readonly string _profilePath;
        private readonly int _count;
        private readonly bool _dry;
        private readonly bool _history;

        public SeedDatabaseCommand(AppOptions options, string database,
            string profilePath, int count, bool dry, bool history)
        {
            _config = options.Configuration;
            _repositoryService = new RepositoryService(_config);
            _seederService = new SeederService();
            _database = database
                ?? throw new ArgumentNullException(nameof(database));
            _profilePath = profilePath
                ?? throw new ArgumentNullException(nameof(profilePath));
            _count = count;
            _dry = dry;
            _history = history;
        }

        public static void Configure(CommandLineApplication command,
            AppOptions options)
        {
            command.Description = "Create and seed a Cadmus MongoDB database " +
                                  "from the specified profile and number of items.";
            command.HelpOption("-?|-h|--help");

            CommandArgument databaseArgument = command.Argument("[database]",
                "The database name");

            CommandArgument profileArgument = command.Argument("[profile]",
                "The seed profile JSON file path");

            CommandOption countOption = command.Option("-c|--count",
                "Items count (default=100)",
                CommandOptionType.SingleValue);

            CommandOption dryOption = command.Option("-d|--dry", "Dry run",
                CommandOptionType.NoValue);

            CommandOption historyOption = command.Option("-h|--history",
                "Add history data",
                CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                int count = 100;
                if (countOption.HasValue())
                {
                    int.TryParse(countOption.Value(), out count);
                }

                options.Command = new SeedDatabaseCommand(options,
                    databaseArgument.Value,
                    profileArgument.Value,
                    count,
                    dryOption.HasValue());
                return 0;
            });
        }

        private static string LoadProfile(string path)
        {
            using StreamReader reader = File.OpenText(path);
            return reader.ReadToEnd();
        }

        public Task Run()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("SEED DATABASE\n");
            Console.ResetColor();

            Console.WriteLine($"Database: {_database}\n" +
                              $"Profile file: {_profilePath}\n" +
                              $"Count: {_count}\n" +
                              $"Dry run: {_dry}\n" +
                              $"History: {_history}\n");
            Serilog.Log.Information("SEED DATABASE: " +
                         $"Database: {_database}, " +
                         $"Profile file: {_profilePath}, " +
                         $"Count: {_count}, " +
                         $"Dry: {_dry}, " +
                         $"History: {_history}");

            if (!_dry)
            {
                // create database if not exists
                string connection = string.Format(CultureInfo.InvariantCulture,
                    _config.GetConnectionString("Mongo"),
                    _database);

                IDatabaseManager manager = new MongoDatabaseManager();

                string profileContent = LoadProfile(_profilePath);
                IDataProfileSerializer serializer = new JsonDataProfileSerializer();
                DataProfile profile = serializer.Read(profileContent);

                if (!manager.DatabaseExists(connection))
                {
                    Console.WriteLine("Creating database...");
                    Serilog.Log.Information($"Creating database {_database}...");

                    manager.CreateDatabase(connection, profile);

                    Console.WriteLine("Database created.");
                    Serilog.Log.Information("Database created.");
                }
            }

            Console.WriteLine("Creating repository...");
            Serilog.Log.Information("Creating repository...");

            ICadmusRepository repository = _dry
                ? null : _repositoryService.CreateRepository(_database);

            Console.WriteLine("Seeding items");
            PartSeederFactory factory = _seederService.GetFactory(_profilePath);
            CadmusSeeder seeder = new CadmusSeeder(factory);
            foreach (IItem item in seeder.GetItems(_count))
            {
                Console.WriteLine($"{item}: {item.Parts.Count} parts");
                if (!_dry)
                {
                    repository.AddItem(item, _history);
                    foreach (IPart part in item.Parts)
                    {
                        repository.AddPart(part, _history);
                    }
                }
            }
            Console.WriteLine("Completed.");

            return Task.CompletedTask;
        }
    }
}
