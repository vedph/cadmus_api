﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using CadmusApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CadmusApi.Controllers
{
    /// <summary>
    /// Item facets controller.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    // [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
    [ApiController]
    public sealed class FacetController : Controller
    {
        private readonly RepositoryService _repositoryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FacetController"/> class.
        /// </summary>
        /// <param name="repositoryService">The repository service.</param>
        /// <exception cref="ArgumentNullException">repository</exception>
        public FacetController(RepositoryService repositoryService)
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
        public IActionResult Get(string database)
        {
            ICadmusRepository repository = _repositoryService.CreateRepository(database);
            return Ok(repository.GetFacets());
        }

        /// <summary>
        /// Gets the list of all the unique parts defined in items facets.
        /// </summary>
        /// <param name="database">The name of the Mongo database.</param>
        /// <param name="noRoles">True to ignore the roles when collecting parts from facets.
        /// In this case, you will get just 1 part for each part type.</param>
        /// <returns>list of parts</returns>
        [HttpGet("api/{database}/facets/parts")]
        public IActionResult GetFacetParts(string database, [FromQuery] bool noRoles = false)
        {
            List<PartDefinition> defs = new List<PartDefinition>();
            ICadmusRepository repository = _repositoryService.CreateRepository(database);
            foreach (IFacet facet in repository.GetFacets())
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
                .ThenBy(d => d.RoleId).ToList());
        }
    }
}
