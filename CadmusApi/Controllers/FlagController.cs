using System;
using System.Linq;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using CadmusApi.Core;
using CadmusApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CadmusApi.Controllers
{
    /// <summary>
    /// Items flags controller.
    /// </summary>
    [Authorize]
    [ApiController]
    public sealed class FlagController : Controller
    {
        private readonly IRepositoryService _repositoryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlagController"/> class.
        /// </summary>
        /// <param name="repositoryService">The repository service.</param>
        /// <exception cref="ArgumentNullException">repository</exception>
        public FlagController(IRepositoryService repositoryService)
        {
            _repositoryService = repositoryService ??
                                 throw new ArgumentNullException(nameof(repositoryService));
        }

        /// <summary>
        /// Gets the list of all the items flags definitions.
        /// </summary>
        /// <param name="database">The name of the Mongo database.</param>
        /// <returns>list of flags definitions</returns>
        [HttpGet("api/{database}/flags")]
        public ActionResult<FlagDefinition[]> Get(string database)
        {
            ICadmusRepository repository = _repositoryService.CreateRepository(database);
            return Ok(repository.GetFlagDefinitions().ToArray());
        }
    }
}
