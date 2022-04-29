using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Serilog.Events;
using Cadmus.Api.Services.Seeding;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Cadmus.Api.Services;

namespace CadmusApi
{
    /// <summary>
    /// Program.
    /// </summary>
    public sealed class Program
    {
        private static void DumpEnvironmentVars()
        {
            Console.WriteLine("ENVIRONMENT VARIABLES:");
            IDictionary dct = Environment.GetEnvironmentVariables();
            List<string> keys = new();
            var enumerator = dct.GetEnumerator();
            while (enumerator.MoveNext())
            {
                keys.Add(((DictionaryEntry)enumerator.Current).Key.ToString());
            }

            foreach (string key in keys.OrderBy(s => s))
                Console.WriteLine($"{key} = {dct[key]}");
        }

        /// <summary>
        /// Creates the host builder.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    // webBuilder.UseSerilog();
                });

        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
#if DEBUG
                .WriteTo.File("cadmus-log.txt", rollingInterval: RollingInterval.Day)
#endif
                .CreateLogger();

            try
            {
                Log.Information("Starting Cadmus API host");
                DumpEnvironmentVars();

                // this is the place for seeding:
                // see https://stackoverflow.com/questions/45148389/how-to-seed-in-entity-framework-core-2
                // and https://docs.microsoft.com/en-us/aspnet/core/migration/1x-to-2x/?view=aspnetcore-2.1#move-database-initialization-code
                var host = await CreateHostBuilder(args)
                    // add in-memory config to override Serilog connection string
                    // as there is no way of configuring it outside appsettings
                    // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-5.0#in-memory-provider-and-binding-to-a-poco-class
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        IConfiguration cfg = AppConfigReader.Read();
                        string csTemplate = cfg.GetValue<string>("Serilog:ConnectionString");
                        string dbName = cfg.GetValue<string>("DatabaseNames:Data");
                        string cs = string.Format(csTemplate, dbName);
                        Debug.WriteLine($"Serilog:ConnectionString override = {cs}");
                        Console.WriteLine($"Serilog:ConnectionString override = {cs}");

                        Dictionary<string, string> dct = new()
                        {
                            { "Serilog:ConnectionString", cs }
                        };
                        // (requires Microsoft.Extensions.Configuration package
                        // to get the MemoryConfigurationProvider)
                        config.AddInMemoryCollection(dct);
                    })
                    .UseSerilog()
                    .Build()
                    .SeedAsync(); // see Services/HostSeedExtension

                host.Run();

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Cadmus API host terminated unexpectedly");
                Debug.WriteLine(ex.ToString());
                Console.WriteLine(ex.ToString());
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
