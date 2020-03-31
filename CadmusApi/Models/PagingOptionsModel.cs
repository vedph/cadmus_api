using Fusi.Tools.Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace CadmusApi.Models
{
    /// <summary>
    /// Paging options model.
    /// </summary>
    public sealed class PagingOptionsModel : IPagingOptions
    {
        /// <summary>
        /// The page number (1-N).
        /// </summary>
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; }

        /// <summary>
        /// The page size (0-100).
        /// </summary>
        [Range(0, 100)]
        public int PageSize { get; set; }
    }
}
