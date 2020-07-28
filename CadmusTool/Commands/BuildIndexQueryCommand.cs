using Cadmus.Index.Sql;
using Fusi.Tools.Data;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace CadmusTool.Commands
{
    public sealed class BuildIndexQueryCommand : ICommand
    {
        private readonly string _dbType;
        private readonly string _query;
        private readonly PagingOptions _options;
        private SqlQueryBuilderBase _builder;

        public BuildIndexQueryCommand(string dbType, string query)
        {
            _dbType = dbType;
            _query = query;
            _options = new PagingOptions();
        }

        public static void Configure(CommandLineApplication command,
            AppOptions options)
        {
            command.Description = "Build Cadmus index SQL query code " +
                                  "from the specified query text.";
            command.HelpOption("-?|-h|--help");

            CommandArgument dbTypeArgument = command.Argument("[db-type]",
                "The database type (mysql or mssql)");
            CommandOption queryOption = command.Option("-q|--query",
                "The query text", CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                options.Command = new BuildIndexQueryCommand(
                    dbTypeArgument.Value,
                    queryOption.Value());
                return 0;
            });
        }

        public Task Run()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("BUILD INDEX QUERY");
            Console.WriteLine();
            Console.ResetColor();

            switch (_dbType?.ToLowerInvariant())
            {
                case "mysql":
                    _builder = new MySqlQueryBuilder();
                    break;
                case "mssql":
                    _builder = new MsSqlQueryBuilder();
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Unknown DB type: {_dbType}");
                    Console.ResetColor();
                    return Task.CompletedTask;
            }

            if (_query != null)
            {
                Console.WriteLine(_query);
                Console.WriteLine();
                var sql = _builder.BuildForItem(_query, _options);
                Console.WriteLine(sql.Item1);
                return Task.CompletedTask;
            }

            while (true)
            {
                Console.Write("Enter query: ");
                string query = Console.ReadLine();
                if (string.IsNullOrEmpty(query) || query == "quit") break;
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                var sql = _builder.BuildForItem(query, _options);
                Console.WriteLine(sql.Item1);
                Console.ResetColor();
            }
            return Task.CompletedTask;
        }
    }
}
