using CadmusApi.Models;
using CadmusApi.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CadmusApi
{
    /// <summary>
    /// Database seed initializer.
    /// See https://stackoverflow.com/questions/45148389/how-to-seed-in-entity-framework-core-2.
    /// </summary>
    public static class DatabaseSeedInitializer
    {
        /// <summary>
        /// Seeds the specified host.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <returns>host</returns>
        public static IWebHost Seed(this IWebHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                IServiceProvider serviceProvider = scope.ServiceProvider;

                try
                {
                    Task.Run(async () =>
                    {
                        var dataseed = new MongoDatabaseInitializer(
                            serviceProvider.GetService<IConfiguration>(),
                            serviceProvider.GetService<UserManager<ApplicationUser>>(),
                            serviceProvider.GetService<RoleManager<ApplicationRole>>());
                        await dataseed.SeedAsync(serviceProvider);
                    }).Wait();
                }
                catch (Exception ex)
                {
                    ILogger<Program> logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred seeding the DB: " +
                        ex.ToString());
                    throw;
                }
            }
            return host;
        }
    }
}
