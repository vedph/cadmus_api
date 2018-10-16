using System;
using System.IO;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using Cadmus.Mongo;
using Microsoft.Extensions.Configuration;

namespace CadmusTool.Services
{
    internal class RepositoryService
    {
        private readonly IConfiguration _configuration;

        public RepositoryService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public ICadmusRepository CreateRepository(string database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));

            // build the tags to types map
            TagToTypeMap map = new TagToTypeMap();
            map.Add(Path.Combine(Directory.GetCurrentDirectory(), "Plugins"), "*parts*.dll");

            // create the repository (no need to use container here)
            MongoCadmusRepository repository = new MongoCadmusRepository(new StandardPartTypeProvider(map));
            repository.Configure(new MongoCadmusRepositoryOptions
            {
                ConnectionStringTemplate = _configuration.GetConnectionString("Mongo"),
                DatabaseName = database
            });

            return repository;
        }
    }
}
