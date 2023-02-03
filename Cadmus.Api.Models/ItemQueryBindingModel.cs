using System;
using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Models;

/// <summary>
/// Item search query binding model.
/// </summary>
public sealed class ItemQueryBindingModel
{
    /// <summary>
    /// Gets or sets the query.
    /// </summary>
    // [Required]
    [MaxLength(1000)]
    public string? Query { get; set; }

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
}
