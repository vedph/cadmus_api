using Cadmus.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cadmus.Api.Models;

/// <summary>
/// Thesaurus model.
/// </summary>
public sealed class ThesaurusModel
{
    /// <summary>
    /// Gets or sets the thesaurus identifier.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the language.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Gets or sets the entries.
    /// </summary>
    public IList<ThesaurusEntry>? Entries { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ThesaurusModel"/> class.
    /// </summary>
    public ThesaurusModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ThesaurusModel"/> class.
    /// </summary>
    /// <param name="thesaurus">The thesaurus.</param>
    public ThesaurusModel(Thesaurus thesaurus)
    {
        ArgumentNullException.ThrowIfNull(thesaurus);

        Id = thesaurus.Id;
        Language = thesaurus.GetLanguage();
        Entries = thesaurus.Entries?.ToArray() ?? Array.Empty<ThesaurusEntry>();
    }
}
