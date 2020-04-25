using Cadmus.Core;
using Cadmus.Index;
using Cadmus.Index.Config;
using Cadmus.Index.Sql;
using CadmusApi.Models;
using CadmusApi.Services;
using Fusi.Tools.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace CadmusApi.Controllers
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

        private ISqlQueryBuilder GetSqlBuilder()
        {
            string type = _configuration.GetValue<string>("Indexing:DatabaseType");
            switch (type?.ToLowerInvariant())
            {
                case "mysql":
                    return new MySqlQueryBuilder();
                case "mssql":
                    return new MsSqlQueryBuilder();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Searches the items index using the specified query, returning
        /// the specified page of results.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="options">The paging options.</param>
        [HttpPost("api/{database}/search")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Search(
            [FromBody] string query,
            [FromQuery] PagingOptionsModel options)
        {
            // build SQL
            ISqlQueryBuilder builder = GetSqlBuilder();
            Tuple<string, string> sql;
            PagingOptions pagingOptions = new PagingOptions
            {
                PageNumber = options.PageNumber,
                PageSize = options.PageSize
            };
            try
            {
                sql = builder.Build(query, pagingOptions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            // get reader
            ItemIndexFactory factory =
                await ItemIndexHelper.GetIndexFactoryAsync(
                    _configuration, _serviceProvider);
            IItemIndexReader reader = factory.GetItemIndexReader();

            // search
            DataPage<ItemInfo> page = reader.Search(query, pagingOptions);
            reader.Close();

            return Ok(page);
        }
    }
}
