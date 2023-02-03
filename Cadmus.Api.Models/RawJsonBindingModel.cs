using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Models;

/// <summary>
/// Raw JSON binding model.
/// </summary>
public sealed class RawJsonBindingModel
{
    /// <summary>
    /// The raw JSON.
    /// </summary>
    [Required]
    public string? Raw { get; set; }
}
