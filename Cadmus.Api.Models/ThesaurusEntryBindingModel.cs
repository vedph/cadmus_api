using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Models;

/// <summary>
/// Tag binding model.
/// </summary>
public sealed class ThesaurusEntryBindingModel
{
    /// <summary>
    /// Gets or sets the tag ID.
    /// </summary>
    [Required(ErrorMessage = "Entry ID is required")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the tag human readable name.
    /// </summary>
    [Required(ErrorMessage = "Entry name is required")]
    public string? Value { get; set; }

    /// <summary>
    /// Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return $"{Id}: {Value}";
    }
}
