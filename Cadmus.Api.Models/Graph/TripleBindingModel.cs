using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Models.Graph;

/// <summary>
/// Graph's triple binding model.
/// </summary>
public class TripleBindingModel
{
    /// <summary>
    /// The triple ID, or 0 for a new triple.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The subject's ID.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int SubjectId { get; set; }

    /// <summary>
    /// The predicate's ID.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int PredicateId { get; set; }

    /// <summary>
    /// The object's ID or 0 if it's a literal.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int? ObjectId { get; set; }

    /// <summary>
    /// The object literal value, or null when using <see cref="ObjectId"/>.
    /// </summary>
    [MaxLength(15000)]
    public string? ObjectLiteral { get; set; }

    /// <summary>
    /// The object literal value filtered for indexing.
    /// This is derived from <see cref="ObjectLiteral"/>, filtering text
    /// in some conventional way, and is a performance-oriented addition
    /// to the literal value to allow faster text-based searches. When not
    /// specified and <see cref="ObjectLiteral"/> is not null, this is
    /// automatically filled by the API.
    /// </summary>
    [MaxLength(15000)]
    public string? ObjectLiteralIx { get; set; }

    /// <summary>
    /// The type of the object literal. This corresponds to literal suffixes
    /// after <c>^^</c> in Turtle: e.g. <c>"12.3"^^xs:double</c>.
    /// </summary>
    [MaxLength(100)]
    public string? LiteralType { get; set; }

    /// <summary>
    /// The object literal language. This is meaningful only for string literals,
    /// and usually is an ISO639 code.
    /// </summary>
    [MaxLength(10)]
    public string? LiteralLanguage { get; set; }

    /// <summary>
    /// The numeric value derived from <see cref="ObjectLiteral"/>
    /// when its type is numeric (boolean, integer, floating point, etc.).
    /// This is a performance-oriented addition to the literal value to allow
    /// for faster searches when dealing with numeric values.
    /// </summary>
    public double? LiteralNumber { get; set; }

    /// <summary>
    /// The SID.
    /// </summary>
    [MaxLength(500)]
    public string? Sid { get; set; }

    /// <summary>
    /// The tag.
    /// </summary>
    [MaxLength(50)]
    public string? Tag { get; set; }

    /// <summary>
    /// The default language to assign to triples having a literal object
    /// without a language specifier.
    /// </summary>
    [MaxLength(10)]
    public string? DefaultLanguage { get; set; }
}
