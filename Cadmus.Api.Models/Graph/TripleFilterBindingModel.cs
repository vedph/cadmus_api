using Cadmus.Graph;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Models.Graph;

/// <summary>
/// Triples filter binding model.
/// </summary>
public class TripleFilterBindingModel : PagingOptionsModel
{
    /// <summary>
    /// Gets or sets the object literal regular expression to match.
    /// </summary>
    [MaxLength(500)]
    public string? LiteralPattern { get; set; }

    /// <summary>
    /// Gets or sets the type of the object literal. This corresponds to
    /// literal suffixes after <c>^^</c> in Turtle: e.g.
    /// <c>"12.3"^^xs:double</c>.
    /// </summary>
    public string? LiteralType { get; set; }

    /// <summary>
    /// Gets or sets the object literal language. This is meaningful only
    /// for string literals, and usually is an ISO639 code.
    /// </summary>
    public string? LiteralLanguage { get; set; }

    /// <summary>
    /// Gets or sets the minimum numeric value for a numeric object literal.
    /// </summary>
    public double? MinLiteralNumber { get; set; }

    /// <summary>
    /// Gets or sets the maximum numeric value for a numeric object literal.
    /// </summary>
    public double? MaxLiteralNumber { get; set; }

    /// <summary>
    /// Gets or sets the subject node identifier to match.
    /// </summary>
    public int SubjectId { get; set; }

    /// <summary>
    /// Gets or sets the predicate node identifier which must be matched.
    /// At least 1 of these must match.
    /// </summary>
    public HashSet<int>? PredicateIds { get; set; }

    /// <summary>
    /// Gets or sets the predicate node identifier which must NOT be matched.
    /// None of these must match.
    /// </summary>
    public HashSet<int>? NotPredicateIds { get; set; }

    /// <summary>
    /// Gets or sets a value equal to true to match only triples having
    /// a literal object, false to match only triples having a non-literal
    /// object, or null to disable this filter.
    /// </summary>
    public bool? HasLiteralObject { get; set; }

    /// <summary>
    /// Gets or sets the object identifier to match.
    /// </summary>
    public int ObjectId { get; set; }

    /// <summary>
    /// Gets or sets the sid.
    /// </summary>
    [MaxLength(500)]
    public string? Sid { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="Sid"/> represents
    /// the initial portion of the SID being searched, rather than the
    /// full SID.
    /// </summary>
    public bool IsSidPrefix { get; set; }

    /// <summary>
    /// Gets or sets the tag filter to match. If null, no tag filtering
    /// is applied; if empty, only triples with a null tag are matched;
    /// otherwise, the triples with the same tag must be matched.
    /// </summary>
    [MaxLength(50)]
    public string? Tag { get; set; }

    /// <summary>
    /// Gets or sets the sort order identifier.
    /// </summary>
    [MaxLength(2)]
    public string? Sort { get; set; }

    /// <summary>
    /// Get a triple filter from this binding model.
    /// </summary>
    /// <returns>The filter.</returns>
    public TripleFilter ToTripleFilter()
    {
        return new TripleFilter
        {
            PageNumber = PageNumber,
            PageSize = PageSize,
            LiteralPattern = LiteralPattern,
            LiteralType = LiteralType,
            LiteralLanguage = LiteralLanguage,
            MinLiteralNumber = MinLiteralNumber,
            MaxLiteralNumber = MaxLiteralNumber,
            SubjectId = SubjectId,
            PredicateIds = PredicateIds,
            NotPredicateIds = NotPredicateIds,
            HasLiteralObject = HasLiteralObject,
            ObjectId = ObjectId,
            Sid = Sid,
            IsSidPrefix = IsSidPrefix,
            Tag = Tag
        };
    }
}
