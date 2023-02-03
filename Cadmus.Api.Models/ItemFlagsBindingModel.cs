using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Models;

/// <summary>
/// Items flags binding model.
/// </summary>
public sealed class ItemFlagsBindingModel
{
    /// <summary>
    /// The IDs of the items to set flags for.
    /// </summary>
    [Required(ErrorMessage = "Item IDs not specified")]
    public IList<string>? Ids { get; set; }

    /// <summary>
    /// The flags value to be set.
    /// </summary>
    [Required(ErrorMessage = "Flags value not specified")]
    public int Flags { get; set; }
}
