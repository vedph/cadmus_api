using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace CadmusApi
{
    /// <summary>
    /// Program.
    /// </summary>
    public static class Program
    {
        private static void DumpEnvironment()
        {
            IDictionary dct = Environment.GetEnvironmentVariables();
            List<string> keys = new List<string>();
            var enumerator = dct.GetEnumerator();
            while (enumerator.MoveNext())
            {
                keys.Add(((DictionaryEntry)enumerator.Current).Key.ToString());
            }

            foreach (string key in keys.OrderBy(s => s))
                Console.WriteLine($"{key} = {dct[key]}");
        }

        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static int Main(string[] args)
        {
            DumpEnvironment();

            // see http://www.carlrippon.com/?p=1118
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile(
                    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                    optional: true)
                .Build();

            // https://github.com/serilog/serilog-aspnetcore
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.MSSqlServer(configuration["Serilog:ConnectionString"],
                    configuration["Serilog:TableName"],
                    autoCreateSqlTable: true)
                .CreateLogger();

            try
            {
                BuildWebHost(args).Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        // no need to customize configuration in startup:
        // https://joonasw.net/view/aspnet-core-2-configuration-changes

        /// <summary>
        /// Builds the web host.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>wen host</returns>
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog()
                .Build();
    }
}
