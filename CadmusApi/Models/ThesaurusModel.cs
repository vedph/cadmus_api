using Cadmus.Core.Config;

namespace CadmusApi.Models
{
    /// <summary>
    /// Thesaurus model.
    /// </summary>
    public sealed class ThesaurusModel
    {
        /// <summary>
        /// Gets or sets the thesaurus identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the entries.
        /// </summary>
        public ThesaurusEntry[] Entries { get; set; }
    }
}
