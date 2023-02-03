using Cadmus.Core.Storage;
using System;
using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Models;

/// <summary>
/// Item filter.
/// </summary>
public sealed class ItemFilterModel
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
    /// Any part of the item's title to be matched.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Any part of the item's description to be matched.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The ID of the item's facet to be matched.
    /// </summary>
    public string? FacetId { get; set; }

    /// <summary>
    /// Gets or sets the group ID to be matched.
    /// </summary>
    public string? GroupId { get; set; }

    /// <summary>
    /// The flag(s) to be matched.
    /// </summary>
    public int? Flags { get; set; }

    /// <summary>
    /// The matching mode used for <see cref="Flags"/>, when specified.
    /// </summary>
    public FlagMatching FlagMatching { get; set; }

    /// <summary>
    /// Minimum modified date and time.
    /// </summary>
    public DateTime? MinModified { get; set; }

    /// <summary>
    /// Maximum modified date and time.
    /// </summary>
    public DateTime? MaxModified { get; set; }

    /// <summary>
    /// The ID of the user who authored the item.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemFilterModel"/> class.
    /// </summary>
    public ItemFilterModel()
    {
        PageNumber = 1;
        PageSize = 20;
    }
}
