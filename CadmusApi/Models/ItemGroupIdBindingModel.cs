using System.ComponentModel.DataAnnotations;

namespace CadmusApi.Models
{
    /// <summary>
    /// Items group ID binding model.
    /// </summary>
    public sealed class ItemGroupIdBindingModel
    {
        /// <summary>
        /// The IDs of the items to set flags for.
        /// </summary>
        [Required(ErrorMessage = "Item IDs not specified")]
        public string[] Ids { get; set; }

        /// <summary>
        /// The group ID to be set or null.
        /// </summary>
        public string GroupId { get; set; }
    }
}
