using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Models.Graph;

/// <summary>
/// Graph node binding model.
/// </summary>
public class NodeBindingModel
{
    /// <summary>
    /// The node's URI identifier.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string? Uri { get; set; }

    /// <summary>
    /// A value indicating whether this node is a class.
    /// This is a shortcut property for a node being the subject of a triple
    /// with S=class URI, predicate="a" and object=rdfs:Class (or eventually
    /// owl:Class -- note that owl:Class is defined as a subclass of
    /// rdfs:Class).
    /// </summary>
    public bool IsClass { get; set; }

    /// <summary>
    /// Gets or sets the tag, used as a generic classification for nodes.
    /// For instance, this can be used to mark all the nodes potentially
    /// used as properties, so that a frontend can filter them accordingly.
    /// </summary>
    [MaxLength(50)]
    public string? Tag { get; set; }

    /// <summary>
    /// Gets or sets the optional node's label. Most nodes have a label
    /// to ease their editing.
    /// </summary>
    [MaxLength(1000)]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the type of the source for this node.
    /// </summary>
    public int SourceType { get; set; }

    /// <summary>
    /// Gets or sets the source ID for this node.
    /// </summary>
    [MaxLength(500)]
    public string? Sid { get; set; }
}
