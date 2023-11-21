using Cadmus.Core;
using Cadmus.Core.Storage;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Cadmus.Api.Services.Seeding;

/// <summary>
/// Parts and items importer. This is used to import parts and their items
/// from JSON files into a Cadmus database.
/// </summary>
public sealed class PartImporter
{
    private readonly JsonDocumentOptions _options;
    private readonly ICadmusRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartImporter"/> class.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <exception cref="ArgumentNullException">repository</exception>
    public PartImporter(ICadmusRepository repository)
    {
        _options = new JsonDocumentOptions
        {
            AllowTrailingCommas = true
        };
        _repository = repository ??
            throw new ArgumentNullException(nameof(repository));
    }

    private static IItem ReadItem(JsonElement itemElem)
    {
        return new Item
        {
            Id = itemElem.GetProperty("id").GetString()!,
            Title = itemElem.GetProperty("title").GetString()!,
            Description = itemElem.GetProperty("description").GetString()!,
            FacetId = itemElem.GetProperty("facetId").GetString()!,
            GroupId = itemElem.GetProperty("groupId").GetString(),
            SortKey = itemElem.GetProperty("sortKey").GetString()!,
            TimeCreated = itemElem.GetProperty("timeCreated").GetDateTime(),
            CreatorId = itemElem.GetProperty("creatorId").GetString()!,
            TimeModified = itemElem.GetProperty("timeModified").GetDateTime(),
            UserId = itemElem.GetProperty("userId").GetString()!,
            Flags = itemElem.GetProperty("flags").GetInt32()
        };
    }

    private static bool JsonHasItems(JsonDocument doc)
    {
        // a JSON array whose entries are items has the "parts" property
        // in its entries
        JsonElement firstEntry = doc.RootElement.EnumerateArray().FirstOrDefault();

        // if empty result really won't matter
        if (firstEntry.Equals(default(JsonElement))) return false;

        return firstEntry.TryGetProperty("parts", out JsonElement _);
    }

    private int ImportItemsWithParts(JsonDocument doc)
    {
        int count = 0;

        // for each item
        foreach (JsonElement itemElem in doc.RootElement.EnumerateArray())
        {
            // read its metadata
            IItem item = ReadItem(itemElem);

            // import it
            _repository.AddItem(item);

            // import its parts
            foreach (JsonElement partElem in itemElem.GetProperty("parts")
                .EnumerateArray())
            {
                _repository.AddPartFromContent(partElem.ToString());
                count++;
            }
        }
        return count;
    }

    private int ImportParts(JsonDocument doc)
    {
        int count = 0;
        foreach (JsonElement partElem in doc.RootElement.EnumerateArray())
        {
            _repository.AddPartFromContent(partElem.ToString());
            count++;
        }
        return count;
    }

    /// <summary>
    /// Imports the specified stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>The count of imported parts.</returns>
    public int Import(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        JsonDocument doc = JsonDocument.Parse(stream, _options);
        return JsonHasItems(doc)? ImportItemsWithParts(doc) : ImportParts(doc);
    }
}
