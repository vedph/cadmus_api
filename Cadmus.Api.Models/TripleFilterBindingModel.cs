using Cadmus.Index.Graph;
using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Models
{
    /// <summary>
    /// Triples filter binding model.
    /// </summary>
    public class TripleFilterBindingModel : PagingOptionsModel
    {
        /// <summary>
        /// Gets or sets the subject node identifier to match.
        /// </summary>
        public int SubjectId { get; set; }

        /// <summary>
        /// Gets or sets the predicate node identifier to match.
        /// </summary>
        public int PredicateId { get; set; }

        /// <summary>
        /// Gets or sets the object identifier to match.
        /// </summary>
        public int ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the object literal regular expression to match.
        /// </summary>
        [MaxLength(15000)]
        public string ObjectLiteral { get; set; }

        /// <summary>
        /// Gets or sets the sid.
        /// </summary>
        [MaxLength(500)]
        public string Sid { get; set; }

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
        public string Tag { get; set; }

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
                SubjectId = SubjectId,
                PredicateId = PredicateId,
                ObjectId = ObjectId,
                ObjectLiteral = ObjectLiteral,
                Sid = Sid,
                IsSidPrefix = IsSidPrefix,
                Tag = Tag
            };
        }
    }
}
