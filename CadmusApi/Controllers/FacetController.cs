using System;
using System.Collections.Generic;
using System.Linq;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using CadmusApi.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CadmusApi.Controllers
{
    /// <summary>
    /// Item facets controller.
    /// </summary>
    [Authorize]
    [ApiController]
    public sealed class FacetController : Controller
    {
        private readonly IRepositoryService _repositoryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FacetController"/> class.
        /// </summary>
        /// <param name="repositoryService">The repository service.</param>
        /// <exception cref="ArgumentNullException">repository</exception>
        public FacetController(IRepositoryService repositoryService)
        {
            _repositoryService = repositoryService ??
                throw new ArgumentNullException(nameof(repositoryService));
        }

        /// <summary>
        /// Gets the list of all the items facets.
        /// </summary>
        /// <param name="database">The name of the Mongo database.</param>
        /// <returns>list of facets</returns>
        [HttpGet("api/{database}/facets")]
        public ActionResult<FacetDefinition[]> Get(string database)
        {
            ICadmusRepository repository = _repositoryService.CreateRepository(database);
            return Ok(repository.GetFacetDefinitions().ToArray());
        }

        /// <summary>
        /// Gets the list of all the unique parts defined in items facets.
        /// </summary>
        /// <param name="database">The name of the Mongo database.</param>
        /// <param name="noRoles">True to ignore the roles when collecting parts from facets.
        /// In this case, you will get just 1 part for each part type.</param>
        /// <returns>list of parts</returns>
        [HttpGet("api/{database}/facets/parts")]
        public ActionResult<PartDefinition[]> GetFacetParts(string database,
            [FromQuery] bool noRoles = false)
        {
            List<PartDefinition> defs = new List<PartDefinition>();
            ICadmusRepository repository = _repositoryService.CreateRepository(database);
            foreach (FacetDefinition facet in repository.GetFacetDefinitions())
            {
                foreach (PartDefinition def in facet.PartDefinitions)
                {
                    if (noRoles && defs.Any(d => d.TypeId == def.TypeId)) continue;
                    if (noRoles) def.RoleId = null;
                    defs.Add(def);
                }
            }

            return Ok(defs.OrderBy(d => d.SortKey)
                .ThenBy(d => d.TypeId)
                .ThenBy(d => d.RoleId).ToArray());
        }
    }
}
