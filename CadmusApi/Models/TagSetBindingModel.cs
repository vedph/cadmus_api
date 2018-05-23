using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CadmusApi.Models
{
    /// <summary>
    /// Tags set binding model.
    /// </summary>
    public sealed class TagSetBindingModel
    {
        /// <summary>
        /// Gets or sets the tag set ID including its language suffix (<c>@xx</c>).
        /// </summary>
        [Required(ErrorMessage = "Tag set ID required")]
        [RegularExpression(@"^[a-zA-Z0-9_\-\.]+\@[a-z]{2}$", ErrorMessage = "Invalid tag set ID")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the tags in this set.
        /// </summary>
        public List<TagBindingModel> Tags { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{Id}: {Tags?.Count}";
        }
    }
}
