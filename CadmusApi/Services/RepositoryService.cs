using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.IO;
using System.Reflection;
using Cadmus.Core.Blocks;
using Cadmus.Core.Config;
using Cadmus.Core.Layers;
using Cadmus.Core.Storage;
using Cadmus.Mongo;
using Fusi.Config;
using Fusi.Config.Sources;
using Microsoft.Extensions.PlatformAbstractions;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace CadmusApi.Services
{
    /// <summary>
    /// Cadmus storage service.
    /// </summary>
    public sealed class RepositoryService
    {
        private static readonly RamConfigSource _source = new RamConfigSource();
        private PluginCatalog _catalog;
        private IConfigWrapper _configWrapper;
        private CompositionHost _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <exception cref="ArgumentNullException">configuration</exception>
        public RepositoryService(IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var dct = _source.Get();
            dct["tagmapbuilder:directory"] = GetPluginDirectory();
            dct["tagmapbuilder:filemask"] = "*parts*.dll";
            dct["repository:connectionstringtemplate"] = configuration["Mongo:Template"];

            _source.Set(dct);
            Configure();
        }

        //private static string BuildSectionKey(string typeName)
        //{
        //    string s = typeName.ToLowerInvariant();
        //    return (s.EndsWith("options") ? s.Substring(0, s.Length - 7) : s);
        //}

        private static string GetPluginDirectory()
        {
            return Path.Combine(
                PlatformServices.Default.Application.ApplicationBasePath,
                "Plugins");
        }

        private void Configure()
        {
            string pluginDir = GetPluginDirectory();

            // we're using the plugins catalog for both parts (IPart)
            // and repositories (MongoDB, which is an additional assembly
            // as we do not intend to change it dynamically, but only
            // take advantage of the catalog DI mechanism for its options)
            _catalog = new PluginCatalog(new PluginAssemblyLoader(),
                new PluginCatalogOptions
                {
                    Directory = pluginDir,
                    FileMask = "*parts*.dll"
                }, new[] { typeof(IPart), typeof(ITextLayerFragment) });

            // MEF2 conventions
            ConventionBuilder cb = new ConventionBuilder();

            // the parts provider, which requires a tag map builder
            cb.ForType<TagMapBuilderOptions>().Export();
            cb.ForType<TagMapBuilder>().Export();
            cb.ForType<StaticDirectoryPartTypeProvider>().Export<IPartTypeProvider>();

            // the repository, which requires the parts provider
            cb.ForType<MongoCadmusRepositoryOptions>().Export();
            cb.ForType<MongoCadmusRepository>().Export<ICadmusRepository>();

            // get a MEF2 container for the catalog plugins
            _container = _catalog.GetContainer(true,
                new CatalogAdditions(new[]
                {
                    typeof(PluginCatalogOptions).GetTypeInfo().Assembly,
                    // typeof(PluginAssemblyLoader).GetTypeInfo().Assembly, (same asm as above)
                    typeof(StaticDirectoryPartTypeProvider).GetTypeInfo().Assembly,
                    typeof(MongoCadmusRepository).GetTypeInfo().Assembly
                }, cb),
                false,
                "cadmus-mongo");

            // define configuration
            _configWrapper = _container.GetExport<IConfigWrapper>();
            // map POCO options to config sections
            _configWrapper.Instance.Configure(typeof(TagMapBuilderOptions), "tagmapbuilder");
            _configWrapper.Instance.Configure(typeof(MongoCadmusRepositoryOptions), "repository");

            // should the catalog's loader include the MongoDB repository, the code below
            // should be enough to map configuration section names to their POCO options 
            // objects; instead, here the file-based catalog is used only for parts, 
            // so that we manually configure these mappings with the code above.
            //foreach (Type t in _catalog.GetOptionTypesFor<ICadmusRepository>(_container))
            //{
            //    string sKey = BuildSectionKey(t.Name);
            //    _config.Configure(t, sKey);
            //} //efor

            _configWrapper.Instance.AddSource(_source);
        }

        /// <summary>
        /// Gets the part type provider.
        /// </summary>
        /// <returns>part type provider</returns>
        public IPartTypeProvider GetPartTypeProvider()
        {
            return _container.GetExport<IPartTypeProvider>();
        }

        /// <summary>
        /// Creates a Cadmus repository.
        /// </summary>
        /// <param name="database">The database name.</param>
        /// <returns>repository</returns>
        /// <exception cref="ArgumentNullException">null source</exception>
        public ICadmusRepository CreateRepository(string database)
        {
            // update the database name in configuration
            Dictionary<string, string> dct = _source.Get();
            dct["repository:databasename"] = database ??
                                             throw new ArgumentNullException(nameof(database));
            _source.Set(dct);
            _configWrapper.Instance.Refresh();

            // create the repository
            return _container.GetExport<ICadmusRepository>();
            // repository.SetSource(database);
        }
    }
}
