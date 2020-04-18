using Cadmus.Core.Storage;
using Cadmus.Index;
using Cadmus.Index.Config;
using CadmusTool.Services;
using Fusi.Tools;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using ShellProgressBar;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CadmusTool.Commands
{
    public sealed class IndexDatabaseCommand : ICommand
    {
        private readonly IConfiguration _config;
        private readonly RepositoryService _repositoryService;
        private readonly string _database;
        private readonly string _profilePath;
        private readonly bool _clear;

        public IndexDatabaseCommand(AppOptions options, string database,
            string profilePath, bool clear)
        {
            _config = options.Configuration;
            _repositoryService = new RepositoryService(_config);
            _database = database
                ?? throw new ArgumentNullException(nameof(database));
            _profilePath = profilePath
                ?? throw new ArgumentNullException(nameof(profilePath));
            _clear = clear;
        }

        public static void Configure(CommandLineApplication command,
            AppOptions options)
        {
            command.Description = "Index a Cadmus MongoDB database " +
                                  "from the specified indexer profile.";
            command.HelpOption("-?|-h|--help");

            CommandArgument databaseArgument = command.Argument("[database]",
                "The database name");

            CommandArgument profileArgument = command.Argument("[profile]",
                "The indexer profile JSON file path");

            CommandOption clearOption = command.Option("-c|--clear",
                "Clear before indexing", CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                options.Command = new IndexDatabaseCommand(options,
                    databaseArgument.Value,
                    profileArgument.Value,
                    clearOption.HasValue());
                return 0;
            });
        }

        private static string LoadProfile(string path)
        {
            using StreamReader reader = File.OpenText(path);
            return reader.ReadToEnd();
        }

        public async Task Run()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("INDEX DATABASE\n");
            Console.ResetColor();

            Console.WriteLine($"Database: {_database}\n" +
                              $"Profile file: {_profilePath}\n" +
                              $"Clear: {_clear}\n");
            Serilog.Log.Information("INDEX DATABASE: " +
                         $"Database: {_database}, " +
                         $"Profile file: {_profilePath}, " +
                         $"Clear: {_clear}");

            string profileContent = LoadProfile(_profilePath);

            string cs = string.Format(_config.GetConnectionString("Index"),
                _database);
            IItemIndexWriterFactoryProvider provider =
                new StaticItemIndexWriterFactoryProvider(cs);
            ItemIndexWriterFactory factory = provider.GetFactory(profileContent);
            IItemIndexWriter writer = factory.GetItemIndexWriter();
            using (var bar = new ProgressBar(100, "Indexing...",
                new ProgressBarOptions
                {
                    ProgressCharacter = '.',
                    ProgressBarOnBottom = true,
                    DisplayTimeInRealTime = false
                }))
            {
                ItemIndexer indexer = new ItemIndexer(writer);
                if (_clear) await indexer.Clear();

                indexer.Build(
                    _repositoryService.CreateRepository(_database),
                    new ItemFilter(),
                    CancellationToken.None,
                    new Progress<ProgressReport>(
                        r => bar.Tick(r.Percent, r.Message)));
            }
            writer.Close();
        }
    }
}

