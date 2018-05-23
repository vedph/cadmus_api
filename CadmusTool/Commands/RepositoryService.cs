using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.IO;
using System.Reflection;
using Cadmus.Core.Blocks;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using Cadmus.Mongo;
using Cadmus.Parts.General;
using Fusi.Config;
using Fusi.Config.Sources;

namespace CadmusTool.Commands
{
    internal static class RepositoryService
    {
        private static string GetPluginDirectory()
        {
            return Path.Combine(
                Path.GetDirectoryName(typeof(RepositoryService).GetTypeInfo().Assembly.Location),
                "Plugins");
        }

        private static string BuildSectionKey(string typeName)
        {
            string s = typeName.ToLowerInvariant();
            return s.EndsWith("options") ? s.Substring(0, s.Length - 7) : s;
        }

        public static ICadmusRepository CreateRepository(string database, string csTemplate)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));

            string pluginDir = GetPluginDirectory();

            PluginCatalog catalog =
                new PluginCatalog(new PluginAssemblyLoader
                    {
                        AdditionalAssemblies = new[]
                        {
                            // this plugin is preloaded because its parts are used by the seeder;
                            // ensure that it's not found in the Plugins folder or this will raise
                            // an error as the assembly is already loaded
                            typeof(NotePart).GetTypeInfo().Assembly
                        }
                    },
                    new PluginCatalogOptions
                    {
                        Directory = pluginDir,
                        FileMask = "*parts*.dll"
                    }, new[] {typeof(IPart)});

            ConventionBuilder cb = new ConventionBuilder();

            // the parts provider, which requires a tag map builder
            cb.ForType<TagMapBuilderOptions>().Export();
            cb.ForType<TagMapBuilder>().Export();
            cb.ForType<DirectoryPartTypeProvider>().Export<IPartTypeProvider>();

            // the repository, which requires the parts provider
            cb.ForType<MongoCadmusRepositoryOptions>().Export();
            cb.ForType<MongoCadmusRepository>().Export<ICadmusRepository>();

            CompositionHost container = catalog.GetContainer(true,
                new CatalogAdditions(new[]
                {
                    typeof(PluginCatalogOptions).GetTypeInfo().Assembly,
                    //typeof(PluginAssemblyLoader).GetTypeInfo().Assembly,
                    typeof(MongoCadmusRepository).GetTypeInfo().Assembly,
                    typeof(DirectoryPartTypeProvider).GetTypeInfo().Assembly    //@@
                }, cb),
                false,
                "cadmus-mongo");

            // define configuration
            MefConfiguration config = container.GetExport<IConfigWrapper>().Instance;
            // source for catalog is in RAM
            config.AddSource(new RamConfigSource
            (new Dictionary<string, string>
            {
                ["tagmapbuilder:directory"] = pluginDir,
                ["tagmapbuilder:filemask"] = "*parts*.dll",
                ["repository:connectionstringtemplate"] = csTemplate,
                ["repository:databasename"] = database
            }));
            foreach (Type t in catalog.GetOptionTypesFor<ICadmusRepository>(container))
            {
                string key = BuildSectionKey(t.Name);
                config.Configure(t, key);
            }

            // Mongo repository
            config.Configure(typeof(TagMapBuilderOptions), "tagmapbuilder");
            config.Configure(typeof(MongoCadmusRepositoryOptions), "repository");

            return container.GetExport<ICadmusRepository>();
        }
    }
}
