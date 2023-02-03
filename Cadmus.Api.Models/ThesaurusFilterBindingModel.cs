using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Models;

/// <summary>
/// Thesaurus filter binding model.
/// </summary>
public sealed class ThesaurusFilterBindingModel
{
    /// <summary>
    /// The page number (1-N).
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; }

    /// <summary>
    /// The page size (1-100).
    /// </summary>
    [Range(1, 100)]
    public int PageSize { get; set; }

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
}
