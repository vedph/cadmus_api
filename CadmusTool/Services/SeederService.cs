using Cadmus.Core.Config;
using Cadmus.Seed;
using Microsoft.Extensions.Configuration;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CadmusTool.Services
{
    internal sealed class SeederService
    {
        private static IList<Assembly> LoadSeedAssemblies(string directory)
        {
            List<Assembly> assemblies = new List<Assembly>();

            foreach (FileInfo file in new DirectoryInfo(directory)
                .GetFiles("Cadmus.Seed.*.dll"))
            {
                assemblies.Add(Assembly.Load(
                    AssemblyName.GetAssemblyName(file.FullName)));
            }

            return assemblies;
        }

        public PartSeederFactory GetFactory(string profilePath)
        {
            if (profilePath == null)
                throw new ArgumentNullException(nameof(profilePath));

            // build the tags to types map for parts/fragments
            TagAttributeToTypeMap map = new TagAttributeToTypeMap();
            map.Add(Path.Combine(Directory.GetCurrentDirectory(), "Plugins"),
                "*parts*.dll");

            // build the container for seeders
            Container container = new Container();
            var seedAssemblies = LoadSeedAssemblies(
                Path.Combine(Directory.GetCurrentDirectory(), "Plugins", "Seed"));

            // configure the seeders container
            PartSeederFactory.ConfigureServices(
                container,
                new StandardPartTypeProvider(map),
                seedAssemblies.ToArray());

            // load seed config
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddJsonFile(profilePath);
            var configuration = builder.Build();

            return new PartSeederFactory(container, configuration);
        }
    }
}
