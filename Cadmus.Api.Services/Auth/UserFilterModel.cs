using System;
using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Services.Auth
{
    /// <summary>
    /// Users filter model.
    /// </summary>
    public sealed class UserFilterModel
    {
        /// <summary>
        /// Any part of the user's nickname or last name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The page number.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; }

        /// <summary>
        /// The page size.
        /// </summary>
        [Required]
        [Range(0, 100)]
        public int PageSize { get; set; }

        /// <summary>
        /// Gets the items skip count corresponding to these paging options.
        /// </summary>
        /// <returns>count of items to skip</returns>
        public int GetSkipCount()
        {
            return PageNumber < 1 ? 0 : (PageNumber - 1) * PageSize;
        }
    }
}
