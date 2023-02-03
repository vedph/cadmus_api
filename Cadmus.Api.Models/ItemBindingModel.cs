using System;
using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Models;

/// <summary>
/// Item binding model.
/// </summary>
public sealed class ItemBindingModel
{
    /// <summary>
    /// Item's identifier (a 32 characters GUID, or null or any text not parsable as
    /// a GUID when it's a new item).
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Item title.
    /// </summary>
    [Required(ErrorMessage = "Title not specified")]
    [MaxLength(500)]
    public string? Title { get; set; }

    /// <summary>
    /// Item short description.
    /// </summary>
    [Required(ErrorMessage = "Description not specified")]
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Item's facet ID.
    /// </summary>
    /// <value>The facet defines which parts can be stored in the item,
    /// and their order and other presentational attributes. It is a unique
    /// string defined in the corpus configuration.</value>
    [Required(ErrorMessage = "Facet ID not specified")]
    [MaxLength(100)]
    public string? FacetId { get; set; }

    /// <summary>
    /// Gets or sets the group identifier. This is an arbitrary string
    /// which can be used to group items into a set. For instance, you
    /// might have a set of items belonging to the same literary work,
    /// a set of lemmata belonging to the same dictionary letter, etc.
    /// </summary>
    [MaxLength(100)]
    public string? GroupId { get; set; }

    /// <summary>
    /// The sort key for the item. This is a value used to sort items in a list.
    /// </summary>
    // [Required(ErrorMessage = "Sort key not specified")]
    [MaxLength(1000)]
    public string? SortKey { get; set; }

    /// <summary>
    /// Gets or sets generic flags for the item.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Invalid flags value")]
    public int Flags { get; set; }
}
