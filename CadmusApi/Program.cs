using System;
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
        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static int Main(string[] args)
        {
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
