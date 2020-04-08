using Cadmus.Core;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using CadmusApi.Models;
using CadmusApi.Services;
using Fusi.Tools.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CadmusApi.Controllers
{
    /// <summary>
    /// Items browsers controller.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Authorize]
    [ApiController]
    public class ItemBrowserController : Controller
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly IItemBrowserFactoryProvider _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemBrowserController" />
        /// class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="provider">The provider.</param>
        /// <exception cref="ArgumentNullException">serviceProvider or
        /// configuration or provider</exception>
        public ItemBrowserController(IServiceProvider serviceProvider,
            IConfiguration configuration,
            IItemBrowserFactoryProvider provider)
        {
            _serviceProvider = serviceProvider ??
                throw new ArgumentNullException(nameof(serviceProvider));
            _configuration = configuration ??
                throw new ArgumentNullException(nameof(configuration));
            _provider = provider ??
                throw new ArgumentNullException(nameof(provider));
        }

        private async Task<ItemBrowserFactory> GetFactory()
        {
            string profileSource = _configuration["Seed:ProfileSource"];
            if (string.IsNullOrEmpty(profileSource)) return null;

            ResourceLoaderService loaderService =
                new ResourceLoaderService(_serviceProvider);

            string profile;
            using (StreamReader reader = new StreamReader(
                await loaderService.LoadAsync(profileSource), Encoding.UTF8))
            {
                profile = reader.ReadToEnd();
            }
            return _provider.GetFactory(profile);
        }

        /// <summary>
        /// Gets a list of all the browser IDs registered in the profile.
        /// </summary>
        /// <returns>Array of strings.</returns>
        [HttpGet("api/browser-ids")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        public async Task<string[]> GetBrowserIds()
        {
            ItemBrowserFactory factory = await GetFactory();
            if (factory == null) return Array.Empty<string>();
            return factory.GetItemBrowserIds();
        }

        /// <summary>
        /// Gets a page of items using the specified browser.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="browserId">The browser identifier.</param>
        /// <param name="model">The model.</param>
        /// <returns>
        /// Items page.
        /// </returns>
        [HttpGet("api/{database}/items-browser/{browserId}")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DataPage<ItemInfo>>> GetItems(
            [FromRoute] string database,
            [FromRoute] string browserId,
            [FromQuery] PagingOptionsModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            IQueryCollection queryString = HttpContext.Request.Query;
            Dictionary<string, string> args = new Dictionary<string, string>();
            foreach (string key in queryString.Keys)
            {
                if (string.Compare(key, "pageNumber", true) != 0
                    && string.Compare(key, "pageSize", true) != 0)
                {
                    string value = queryString[key].ToString();
                    args[key] = value == "null"? null : value;
                }
            }

            ItemBrowserFactory factory = await GetFactory();
            if (factory == null)
            {
                return Ok(new DataPage<ItemInfo>(model.PageNumber,
                    model.PageSize, 0, new List<ItemInfo>()));
            }

            IItemBrowser browser = factory.GetItemBrowser(browserId);
            if (browser == null)
                return NotFound($"Item browser with ID {browserId} not found");

            DataPage<ItemInfo> page = await browser.BrowseAsync(database,
                model, args);
            return Ok(page);
        }
    }
}
