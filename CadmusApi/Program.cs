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
using NpgsqlTypes;
using Serilog.Sinks.PostgreSQL.ColumnWriters;
using Serilog.Sinks.PostgreSQL;
using Fusi.DbManager.PgSql;
using System.Text.RegularExpressions;

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

    private static bool IsAuditEnabledFor(IConfiguration config, string key)
    {
        bool? value = config.GetValue<bool?>($"Auditing:{key}");
        return value != null && value != false;
    }

    private static void ConfigurePostgreLogging(HostBuilderContext context,
        LoggerConfiguration loggerConfiguration)
    {
        string cs = context.Configuration.GetConnectionString("PostgresLog");
        if (string.IsNullOrEmpty(cs))
        {
            Console.WriteLine("Postgres log connection string not found");
            return;
        }

        Regex dbRegex = new("Database=(?<n>[^;]+);?");
        Match m = dbRegex.Match(cs);
        if (!m.Success)
        {
            Console.WriteLine("Postgres log connection string not valid");
            return;
        }
        string cst = dbRegex.Replace(cs, "Database={0};");
        string dbName = m.Groups["n"].Value;
        PgSqlDbManager mgr = new(cst);
        if (!mgr.Exists(dbName))
        {
            Console.WriteLine($"Creating log database {dbName}...");
            mgr.CreateDatabase(dbName, "", null);
        }

        IDictionary<string, ColumnWriterBase> columnWriters =
            new Dictionary<string, ColumnWriterBase>
        {
            { "message", new RenderedMessageColumnWriter(
                NpgsqlDbType.Text) },
            { "message_template", new MessageTemplateColumnWriter(
                NpgsqlDbType.Text) },
            { "level", new LevelColumnWriter(
                true, NpgsqlDbType.Varchar) },
            { "raise_date", new TimestampColumnWriter(
                NpgsqlDbType.TimestampTz) },
            { "exception", new ExceptionColumnWriter(
                NpgsqlDbType.Text) },
            { "properties", new LogEventSerializedColumnWriter(
                NpgsqlDbType.Jsonb) },
            { "props_test", new PropertiesColumnWriter(
                NpgsqlDbType.Jsonb) },
            { "machine_name", new SinglePropertyColumnWriter(
                "MachineName", PropertyWriteMethod.ToString,
                NpgsqlDbType.Text, "l") }
        };

        loggerConfiguration
            .WriteTo.PostgreSQL(cs, "log", columnWriters,
            needAutoCreateTable: true, needAutoCreateSchema: true);
    }

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
                    var maxSize = hostingContext.Configuration["Serilog:MaxMbSize"];

                    loggerConfiguration.ReadFrom.Configuration(
                        hostingContext.Configuration);

                    if (IsAuditEnabledFor(hostingContext.Configuration, "File"))
                    {
                        Console.WriteLine("Logging to file enabled");
                        loggerConfiguration.WriteTo.File("cadmus-log.txt",
                            rollingInterval: RollingInterval.Day);
                    }

                    if (IsAuditEnabledFor(hostingContext.Configuration, "Mongo"))
                    {
                        Console.WriteLine("Logging to Mongo enabled");
                        string cs = hostingContext.Configuration
                            .GetConnectionString("MongoLog");

                        if (!string.IsNullOrEmpty(cs))
                        {
                            loggerConfiguration.WriteTo.MongoDBCapped(cs,
                                cappedMaxSizeMb: !string.IsNullOrEmpty(maxSize) &&
                                int.TryParse(maxSize, out int n) && n > 0 ? n : 10);
                        }
                        else
                        {
                            Console.WriteLine("Mongo log connection string not found");
                        }
                    }

                    if (IsAuditEnabledFor(hostingContext.Configuration, "Postgres"))
                    {
                        Console.WriteLine("Logging to Postgres enabled");
                        ConfigurePostgreLogging(hostingContext, loggerConfiguration);
                    }

                    if (IsAuditEnabledFor(hostingContext.Configuration, "Console"))
                    {
                        Console.WriteLine("Logging to console enabled");
                        loggerConfiguration.WriteTo.Console();
                    }
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
