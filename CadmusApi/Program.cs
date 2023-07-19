using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Cadmus.Api.Services.Seeding;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CadmusApi;

/// <summary>
/// Program.
/// </summary>
public static class Program
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
            });

    /// <summary>
    /// Entry point.
    /// </summary>
    /// <param name="args">The arguments.</param>
    public static async Task<int> Main(string[] args)
    {
        try
        {
            Log.Information("Starting Cadmus API host");
            DumpEnvironmentVars();

            // this is the place for seeding:
            // see https://stackoverflow.com/questions/45148389/how-to-seed-in-entity-framework-core-2
            // and https://docs.microsoft.com/en-us/aspnet/core/migration/1x-to-2x/?view=aspnetcore-2.1#move-database-initialization-code
            var host = await CreateHostBuilder(args)
                .UseSerilog((hostingContext, loggerConfiguration) =>
                {
                    string cs = hostingContext.Configuration
                        .GetConnectionString("Log");
                    var maxSize = hostingContext.Configuration["Serilog:MaxMbSize"];

                    loggerConfiguration
                        .ReadFrom.Configuration(hostingContext.Configuration)
#if DEBUG
                        .WriteTo.File("cadmus-log.txt", rollingInterval: RollingInterval.Day)
#endif
                        .WriteTo.MongoDBCapped(cs,
                            cappedMaxSizeMb: !string.IsNullOrEmpty(maxSize) &&
                                int.TryParse(maxSize, out int n) && n > 0 ? n : 10);
                })
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
