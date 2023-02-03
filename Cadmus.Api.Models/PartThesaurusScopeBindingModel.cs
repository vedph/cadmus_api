using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Models;

/// <summary>
/// Part thesaurus scope binding model.
/// </summary>
public sealed class PartThesaurusScopeBindingModel
{
    /// <summary>
    /// The IDs of the parts to set scope for.
    /// </summary>
    [Required(ErrorMessage = "Part IDs not specified")]
    public IList<string>? Ids { get; set; }

    /// <summary>
    /// The thesaurus scope to be set or null.
    /// </summary>
    public string? Scope { get; set; }
}
