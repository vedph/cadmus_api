using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Models;

/// <summary>
/// Items group ID binding model.
/// </summary>
public sealed class ItemGroupIdBindingModel
{
    /// <summary>
    /// The IDs of the items to set group ID for.
    /// </summary>
    [Required(ErrorMessage = "Item IDs not specified")]
    public IList<string>? Ids { get; set; }

    /// <summary>
    /// The group ID to be set or null.
    /// </summary>
    public string? GroupId { get; set; }
}
