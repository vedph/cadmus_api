using Cadmus.Index.Sql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemIndexController"/>
        /// class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <exception cref="ArgumentNullException">configuration</exception>
        public ItemIndexController(IConfiguration configuration)
        {
            _configuration = configuration ??
                throw new ArgumentNullException(nameof(configuration));
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

        // TODO
    }
}
