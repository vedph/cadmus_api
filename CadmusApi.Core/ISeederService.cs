using Cadmus.Seed;

namespace CadmusApi.Core
{
    /// <summary>
    /// Seeders service.
    /// </summary>
    public interface ISeederService
    {
        /// <summary>
        /// Gets the part/fragment seeders factory.
        /// </summary>
        /// <param name="profilePath">The profile file path.</param>
        /// <returns>Factory.</returns>
        PartSeederFactory GetFactory(string profilePath);
    }
}
