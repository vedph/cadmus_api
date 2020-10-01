using System;
using System.Linq;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using Cadmus.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cadmus.Api.Controllers
{
    /// <summary>
    /// Items flags controller.
    /// </summary>
    [Authorize]
    [ApiController]
    public sealed class FlagController : Controller
    {
        private readonly IRepositoryProvider _repositoryProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlagController"/> class.
        /// </summary>
        /// <param name="repositoryProvider">The repository provider.</param>
        /// <exception cref="ArgumentNullException">repository</exception>
        public FlagController(IRepositoryProvider repositoryProvider)
        {
            _repositoryProvider = repositoryProvider ??
                                 throw new ArgumentNullException(nameof(repositoryProvider));
        }

        /// <summary>
        /// Gets the list of all the items flags definitions.
        /// </summary>
        /// <param name="database">The name of the Mongo database.</param>
        /// <returns>list of flags definitions</returns>
        [HttpGet("api/{database}/flags")]
        public ActionResult<FlagDefinition[]> Get(string database)
        {
            ICadmusRepository repository = _repositoryProvider.CreateRepository(database);
            return Ok(repository.GetFlagDefinitions().ToArray());
        }
    }
}
