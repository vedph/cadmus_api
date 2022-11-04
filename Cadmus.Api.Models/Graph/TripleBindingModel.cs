using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Models.Graph
{
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
        public int ObjectId { get; set; }

        /// <summary>
        /// The object literal value, or null when using <see cref="ObjectId"/>.
        /// </summary>
        [MaxLength(15000)]
        public string? ObjectLiteral { get; set; }

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
    }
}
