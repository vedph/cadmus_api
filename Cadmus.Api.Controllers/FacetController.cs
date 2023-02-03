﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cadmus.Core;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cadmus.Api.Controllers;

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
    /// <returns>list of facets</returns>
    [HttpGet("api/facets")]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    public ActionResult<FacetDefinition[]> Get()
    {
        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();
        return Ok(repository.GetFacetDefinitions().ToArray());
    }

    private static PartDefinition[] CollectFacetParts(
        FacetDefinition facet,
        bool noRoles)
    {
        List<PartDefinition> defs = new();

        if (facet != null)
        {
            foreach (PartDefinition def in facet.PartDefinitions)
            {
                if (noRoles && defs.Any(d => d.TypeId == def.TypeId)) continue;
                if (noRoles) def.RoleId = null;
                defs.Add(def);
            }
        }

        return defs.OrderBy(d => d.SortKey)
            .ThenBy(d => d.TypeId)
            .ThenBy(d => d.RoleId).ToArray();
    }

    /// <summary>
    /// Gets the facet with the specified ID.
    /// </summary>
    /// <param name="id">The facet ID.</param>
    [HttpGet("api/facets/{id}")]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public ActionResult<PartDefinition> GetFacet([FromRoute] string id)
    {
        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();

        FacetDefinition? facet = repository.GetFacetDefinition(id);
        if (facet == null) return NotFound();
        return Ok(facet);
    }

    /// <summary>
    /// Gets the facet assigned to the item with the specified ID.
    /// </summary>
    /// <param name="id">The item ID.</param>
    [HttpGet("api/facets/items/{id}")]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public ActionResult<PartDefinition> GetFacetFromItemId(
        [FromRoute] string id)
    {
        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();

        IItem? item = repository.GetItem(id, false);
        if (item?.FacetId == null) return NotFound();

        FacetDefinition? facet = repository.GetFacetDefinition(item.FacetId);
        if (facet == null) return NotFound();
        return Ok(facet);
    }

    /// <summary>
    /// Gets the list of all the parts defined in the specified item's facet.
    /// </summary>
    /// <param name="id">The facet ID.</param>
    /// <param name="noRoles">True to ignore the roles when collecting
    /// parts from facets.
    /// In this case, you will get just 1 part for each part type.</param>
    /// <returns>List of parts, sorted by their sort key, and then by
    /// type and role IDs, should the sort key not be enough to distinguish
    /// them (which should not happen).</returns>
    [HttpGet("api/facets/{id}/parts")]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    public ActionResult<PartDefinition[]> GetFacetParts(
        [FromRoute] string id,
        [FromQuery] bool noRoles = false)
    {
        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();

        FacetDefinition? facet = repository.GetFacetDefinition(id);
        if (facet == null)
            return Ok(Array.Empty<PartDefinition>());

        PartDefinition[] result = CollectFacetParts(facet, noRoles);
        return Ok(result);
    }

    /// <summary>
    /// Gets the list of all the parts defined in the facet assigned
    /// to the specified item.
    /// </summary>
    /// <param name="id">The item ID. The facet ID will be retrieved
    /// from the item.</param>
    /// <param name="noRoles">True to ignore the roles when collecting
    /// parts from facets.
    /// In this case, you will get just 1 part for each part type.</param>
    /// <returns>List of parts, sorted by their sort key, and then by
    /// type and role IDs, should the sort key not be enough to distinguish
    /// them (which should not happen).</returns>
    [HttpGet("api/item-facets/{id}/parts")]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    public ActionResult<PartDefinition[]> GetFacetPartsFromItem(
        [FromRoute] string id,
        [FromQuery] bool noRoles = false)
    {
        ICadmusRepository repository = _repositoryProvider.CreateRepository();

        IItem? item = repository.GetItem(id, false);
        PartDefinition[] result;

        if (item?.FacetId != null)
        {
            FacetDefinition? facet = repository.GetFacetDefinition(item.FacetId);
            if (facet == null) return Ok(Array.Empty<PartDefinition>());
            result = CollectFacetParts(facet, noRoles);
        }
        else result = Array.Empty<PartDefinition>();

        return Ok(result);
    }

    /// <summary>
    /// Gets the text layer part type identifier.
    /// </summary>
    /// <remarks>In a set of facets, the layer parts have all the same type,
    /// and have a role ID starting with <c>fr.</c>. This method finds
    /// the first part with this role ID, and returns its type ID.</remarks>
    /// <returns>An object with <c>TypeId</c>=type ID or null.</returns>
    [HttpGet("api/facets/layer-type-id")]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    public IActionResult GetTextLayerPartTypeId()
    {
        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();

        foreach (FacetDefinition facet in repository.GetFacetDefinitions())
        {
            PartDefinition? partDef = facet.PartDefinitions
                .Find(d => d.RoleId?.StartsWith(
                    PartBase.FR_PREFIX, StringComparison.Ordinal) == true);
            if (partDef != null)
                return Ok(new { partDef.TypeId });
        }
        return Ok(new { TypeId = (string?)null });
    }
}
