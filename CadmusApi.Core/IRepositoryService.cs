using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using System;

namespace CadmusApi.Core
{
    /// <summary>
    /// Cadmus repository service.
    /// </summary>
    public interface IRepositoryService
    {
        /// <summary>
        /// Gets the part type provider.
        /// </summary>
        /// <returns>part type provider</returns>
        IPartTypeProvider GetPartTypeProvider();

        /// <summary>
        /// Creates a Cadmus repository.
        /// </summary>
        /// <param name="database">The database name.</param>
        /// <returns>repository</returns>
        /// <exception cref="ArgumentNullException">null database</exception>
        ICadmusRepository CreateRepository(string database);
    }
}
