using Cadmus.Api.Services.Seeding;
using Cadmus.Core;
using Cadmus.Index;
using Cadmus.Index.Config;
using Cadmus.Api.Models;
using Fusi.Tools.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Cadmus.Api.Controllers
{
    /// <summary>
    /// Items index.
    /// </summary>
    /// <seealso cref="Controller" />
    [Authorize]
    [ApiController]
    public sealed class ItemIndexController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemIndexController" />
        /// class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <exception cref="ArgumentNullException">configuration or provider</exception>
        public ItemIndexController(IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            _configuration = configuration ??
                throw new ArgumentNullException(nameof(configuration));
            _serviceProvider = serviceProvider ??
                throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Searches the items index using the specified query, returning
        /// the specified page of results.
        /// </summary>
        /// <param name="model">The query model.</param>
        /// <returns>Object with value=items page or error=error message.</returns>
        [HttpPost("api/{database}/search")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Search(
            [FromBody] ItemQueryBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // nope if empty query
            if (string.IsNullOrWhiteSpace(model.Query))
            {
                return Ok(new
                {
                    error = "No query"
                });
            }

            // get reader
            ItemIndexFactory factory =
                await ItemIndexHelper.GetIndexFactoryAsync(
                    _configuration, _serviceProvider);
            IItemIndexReader reader = factory.GetItemIndexReader();

            // search
            DataPage<ItemInfo> page = reader.SearchItems(model.Query,
                new PagingOptions
                {
                    PageNumber = model.PageNumber,
                    PageSize = model.PageSize
                });
            reader.Close();

            return Ok(new
            {
                value = page
            });
        }

        /// <summary>
        /// Searches the pins index using the specified query, returning
        /// the specified page of results.
        /// </summary>
        /// <param name="model">The query model.</param>
        /// <returns>Object with value=items page or error=error message.</returns>
        [HttpPost("api/{database}/pin-search")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SearchPins(
            [FromBody] ItemQueryBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // nope if empty query
            if (string.IsNullOrWhiteSpace(model.Query))
            {
                return Ok(new
                {
                    error = "No query"
                });
            }

            // get reader
            ItemIndexFactory factory =
                await ItemIndexHelper.GetIndexFactoryAsync(
                    _configuration, _serviceProvider);
            IItemIndexReader reader = factory.GetItemIndexReader();

            // search
            DataPage<DataPinInfo> page = reader.SearchPins(model.Query,
                new PagingOptions
                {
                    PageNumber = model.PageNumber,
                    PageSize = model.PageSize
                });
            reader.Close();

            return Ok(new
            {
                value = page
            });
        }
    }
}
