using System;
using System.Reflection;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using Cadmus.Lexicon.Parts;
using Cadmus.Mongo;
using Cadmus.Parts.General;
using Cadmus.Philology.Parts.Layers;
using Microsoft.Extensions.Configuration;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace CadmusApi.Services
{
    /// <summary>
    /// Cadmus storage service.
    /// </summary>
    public sealed class RepositoryService
    {
        private readonly IConfiguration _configuration;
        private readonly TagToTypeMap _map;
        private readonly IPartTypeProvider _partTypeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <exception cref="ArgumentNullException">configuration</exception>
        public RepositoryService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _map = new TagToTypeMap();
            _map.Add(new[]
            {
                // Cadmus.Parts
                typeof(NotePart).GetTypeInfo().Assembly,
                // Cadmus.Lexicon.Parts
                typeof(WordFormPart).GetTypeInfo().Assembly,
                // Cadmus.Philology.Parts
                typeof(ApparatusLayerFragment).GetTypeInfo().Assembly
            });

            _partTypeProvider = new StandardPartTypeProvider(_map);
        }

        /// <summary>
        /// Gets the part type provider.
        /// </summary>
        /// <returns>part type provider</returns>
        public IPartTypeProvider GetPartTypeProvider()
        {
            return _partTypeProvider;
        }

        /// <summary>
        /// Creates a Cadmus repository.
        /// </summary>
        /// <param name="database">The database name.</param>
        /// <returns>repository</returns>
        /// <exception cref="ArgumentNullException">null database</exception>
        public ICadmusRepository CreateRepository(string database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));

            // create the repository (no need to use container here)
            MongoCadmusRepository repository = new MongoCadmusRepository(_partTypeProvider);
            repository.Configure(new MongoCadmusRepositoryOptions
            {
                ConnectionStringTemplate = _configuration["Data:ConnectionString"],
                DatabaseName = database
            });

            return repository;
        }
    }
}
