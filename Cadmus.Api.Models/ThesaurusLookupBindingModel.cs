using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Models;

/// <summary>
/// Thesaurus lookup filter binding model.
/// </summary>
public sealed class ThesaurusLookupBindingModel
{
    /// <summary>
    /// Gets or sets the limit of IDs to be retrieved (0=all).
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// The tag's ID filter. This matches all the thesauri whose ID
    /// contains the specified text.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Alias filter. This is null to match all the thesauri, true to
    /// match only those with a TargetId, false to match only those without
    /// it.
    /// </summary>
    public bool? IsAlias { get; set; }

    /// <summary>
    /// Gets or sets the language filter.
    /// </summary>
    [RegularExpression("^[a-z]{2,3}$", ErrorMessage = "Invalid language filter")]
    public string? Language { get; set; }

    /// <summary>
    /// Determines whether this filter is empty.
    /// </summary>
    public bool IsEmpty() => Limit == 0
        && string.IsNullOrEmpty(Id)
        && IsAlias == null
        && string.IsNullOrEmpty(Language);
}
