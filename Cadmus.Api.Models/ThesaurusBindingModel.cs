using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Models;

/// <summary>
/// Thesaurus binding model.
/// </summary>
public sealed class ThesaurusBindingModel
{
    /// <summary>
    /// Gets or sets the tag set ID including its language suffix (<c>@xx</c>).
    /// </summary>
    [Required(ErrorMessage = "Thesaurus ID required")]
    [RegularExpression(@"^[a-zA-Z0-9_\-\.]+\@[a-z]{2}$",
        ErrorMessage = "Invalid thesaurus ID")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the tags in this set.
    /// </summary>
    public IList<ThesaurusEntryBindingModel>? Entries { get; set; }

    /// <summary>
    /// Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return $"{Id}: {Entries?.Count}";
    }
}
