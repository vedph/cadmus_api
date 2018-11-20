using System;
using System.Threading.Tasks;

namespace CadmusApi.Services
{
    /// <summary>
    /// Database initializer.
    /// </summary>
    public interface IDatabaseInitializer
    {
        /// <summary>
        /// Seeds the database.
        /// </summary>
        Task SeedAsync(IServiceProvider provider);
    }
}
