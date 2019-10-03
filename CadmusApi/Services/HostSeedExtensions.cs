using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Data.Common;
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
                    Task.Run(async () =>
                    {
                        IConfiguration config =
                            serviceProvider.GetService<IConfiguration>();

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
