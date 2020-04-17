using System;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;

namespace CadmusTool.Commands
{
    public sealed class RootCommand : ICommand
    {
        private readonly CommandLineApplication _app;

        public RootCommand(CommandLineApplication app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
        }

        public static void Configure(CommandLineApplication app, AppOptions options)
        {
            // configure all the app commands here
            app.Command("seed",
                c => SeedDatabaseCommand.Configure(c, options));
            app.Command("import-lex",
                c => ImportLexiconCommand.Configure(c, options));
            app.Command("index",
                c => IndexDatabaseCommand.Configure(c, options));

            app.OnExecute(() =>
            {
                options.Command = new RootCommand(app);
                return 0;
            });
        }

        public Task Run()
        {
            _app.ShowHelp();
            return Task.FromResult(0);
        }
    }
}
