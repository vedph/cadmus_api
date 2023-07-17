using System;
using System.Reflection;
using Cadmus.Core;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using Cadmus.General.Parts;
using Cadmus.Img.Parts;
using Cadmus.Mongo;
using Cadmus.Philology.Parts;

namespace CadmusApi.Services;

/// <summary>
/// Application's repository provider. Usually, this is implemented in your
/// project's Services library. Here we have no specific project, so we
/// just provide an API app service here.
/// </summary>
public sealed class AppRepositoryProvider : IRepositoryProvider
{
    private readonly IPartTypeProvider _partTypeProvider;

    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppRepositoryProvider"/> class.
    /// </summary>
    /// <exception cref="ArgumentNullException">configuration</exception>
    public AppRepositoryProvider()
    {
        TagAttributeToTypeMap _map = new();
        _map.Add(new[]
        {
            // Cadmus.General.Parts
            typeof(NotePart).GetTypeInfo().Assembly,
            // Cadmus.Philology.Parts
            typeof(ApparatusLayerFragment).GetTypeInfo().Assembly,
            // Cadmus.Img.Parts
            typeof(W3CGalleryImageAnnotationsPart).GetTypeInfo().Assembly
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
    /// <returns>repository</returns>
    /// <exception cref="ArgumentNullException">null database</exception>
    public ICadmusRepository CreateRepository()
    {
        // create the repository (no need to use container here)
        MongoCadmusRepository repository =
            new(_partTypeProvider, new StandardItemSortKeyBuilder());

        repository.Configure(new MongoCadmusRepositoryOptions
        {
            ConnectionString = ConnectionString ??
                throw new InvalidOperationException(
                "No connection string set for IRepositoryProvider implementation")
        });

        return repository;
    }
}
