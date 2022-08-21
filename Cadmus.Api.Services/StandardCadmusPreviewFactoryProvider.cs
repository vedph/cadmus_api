using Cadmus.Export.Preview;
using Fusi.Microsoft.Extensions.Configuration.InMemoryJson;
using Microsoft.Extensions.Configuration;
using SimpleInjector;
using System;
using System.Reflection;

namespace Cadmus.Api.Services
{
    /// <summary>
    /// Standard Cadmus preview factory provider.
    /// </summary>
    public sealed class StandardCadmusPreviewFactoryProvider :
        ICadmusPreviewFactoryProvider
    {
        /// <summary>
        /// Gets the factory.
        /// </summary>
        /// <param name="profile">The profile.</param>
        /// <param name="additionalAssemblies">The optional additional assemblies.
        /// </param>
        /// <returns>Factory.</returns>
        /// <exception cref="ArgumentNullException">profile</exception>
        public CadmusPreviewFactory GetFactory(string profile,
            params Assembly[] additionalAssemblies)
        {
            if (profile is null) throw new ArgumentNullException(nameof(profile));

            Container container = new();
            CadmusPreviewFactory.ConfigureServices(container, additionalAssemblies);

            ConfigurationBuilder cb = new();
            IConfigurationRoot config = cb
                .AddInMemoryJson(profile)
                .Build();
            return new CadmusPreviewFactory(container, config);
        }
    }
}
