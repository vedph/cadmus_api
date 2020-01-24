using System;
using System.Collections.Generic;
using System.Linq;
using Cadmus.Core;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
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
        private readonly IRepositoryProvider _repositoryProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FacetController"/> class.
        /// </summary>
        /// <param name="repositoryProvider">The repository provider.</param>
        /// <exception cref="ArgumentNullException">repository</exception>
        public FacetController(IRepositoryProvider repositoryProvider)
        {
            _repositoryProvider = repositoryProvider ??
                throw new ArgumentNullException(nameof(repositoryProvider));
        }

        /// <summary>
        /// Gets the list of all the items facets.
        /// </summary>
        /// <param name="database">The name of the Mongo database.</param>
        /// <returns>list of facets</returns>
        [HttpGet("api/{database}/facets")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        public ActionResult<FacetDefinition[]> Get(string database)
        {
            ICadmusRepository repository =
                _repositoryProvider.CreateRepository(database);
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
        [Produces("application/json")]
        [ProducesResponseType(200)]
        public ActionResult<PartDefinition[]> GetFacetParts(string database,
            [FromQuery] bool noRoles = false)
        {
            List<PartDefinition> defs = new List<PartDefinition>();
            ICadmusRepository repository =
                _repositoryProvider.CreateRepository(database);

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

        /// <summary>
        /// Gets the text layer part type identifier.
        /// </summary>
        /// <remarks>In a set of facets, the layer parts have all the same type,
        /// and have a role ID starting with <c>fr.</c>. This method finds
        /// the first part with this role ID, and returns its type ID.</remarks>
        /// <param name="database">The database.</param>
        /// <returns>An object with <c>TypeId</c>=type ID or null.</returns>
        [HttpGet("api/{database}/facets/layer-type-id")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        public IActionResult GetTextLayerPartTypeId(string database)
        {
            ICadmusRepository repository =
                _repositoryProvider.CreateRepository(database);

            foreach (FacetDefinition facet in repository.GetFacetDefinitions())
            {
                PartDefinition partDef = facet.PartDefinitions
                    .Find(d => d.RoleId?.StartsWith(
                        PartBase.FR_PREFIX, StringComparison.Ordinal) == true);
                if (partDef != null)
                    return Ok(new { partDef.TypeId });
            }
            return Ok(new { TypeId = (string)null });
        }
    }
}
