using Cadmus.Core.Config;
using Cadmus.Seed.Parts.General;
using Cadmus.Seed.Philology.Parts.Layers;
using Microsoft.Extensions.Configuration;
using SimpleInjector;
using System;
using System.Reflection;

namespace CadmusApi.Services
{
    /// <summary>
    /// Standard items browser factory provider.
    /// </summary>
    /// <seealso cref="IItemBrowserFactoryProvider" />
    public sealed class StandardItemBrowserFactoryProvider :
        IItemBrowserFactoryProvider
    {
        /// <summary>
        /// Gets the item browsers factory.
        /// </summary>
        /// <param name="profilePath">The profile file path.</param>
        /// <returns>Factory.</returns>
        /// <exception cref="ArgumentNullException">profilePath</exception>
        public ItemBrowserFactory GetFactory(string profilePath)
        {
            if (profilePath == null)
                throw new ArgumentNullException(nameof(profilePath));

            // build the tags to types map for parts/fragments
            Assembly[] seedAssemblies = new[]
            {
                // Cadmus.Seed.Parts
                typeof(NotePartSeeder).Assembly,
                // Cadmus.Seed.Philology.Parts
                typeof(ApparatusLayerFragmentSeeder).Assembly
            };
            TagAttributeToTypeMap map = new TagAttributeToTypeMap();
            map.Add(seedAssemblies);

            // build the container for seeders
            Container container = new Container();
            ItemBrowserFactory.ConfigureServices(
                container,
                new StandardPartTypeProvider(map),
                seedAssemblies);

            container.Verify();

            // load seed configuration
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddJsonFile(profilePath);
            var configuration = builder.Build();

            return new ItemBrowserFactory(
                container,
                configuration,
                configuration.GetConnectionString("Default"));
        }
    }
}
