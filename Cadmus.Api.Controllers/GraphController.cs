using Cadmus.Api.Models.Graph;
using Cadmus.Api.Services.Seeding;
using Cadmus.Graph;
using Cadmus.Index.Config;
using Fusi.Tools.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadmus.Api.Controllers;

/// <summary>
/// Semantic graph.
/// </summary>
[Authorize]
[ApiController]
public sealed class GraphController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphController" />
    /// class.
    /// <param name="serviceProvider"></param>
    /// <param name="configuration"></param>
    public GraphController(IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    #region Nodes
    /// <summary>
    /// Get the specified page of graph nodes.
    /// </summary>
    /// <param name="model">The filter.</param>
    /// <returns>A page of nodes.</returns>
    [HttpGet("api/graph/nodes")]
    [ProducesResponseType(200)]
    public async Task<DataPage<UriNode>> GetNodes([FromQuery]
        NodeFilterBindingModel model)
    {
        ItemGraphFactory factory = (await ItemIndexHelper
            .GetGraphFactoryAsync(_configuration, _serviceProvider))!;

        IGraphRepository repository = factory.GetGraphRepository()!;
        return repository.GetNodes(model.ToNodeFilter());
    }

    /// <summary>
    /// Get the node with the specified ID.
    /// </summary>
    /// <param name="id">The node ID.</param>
    /// <returns>Node.</returns>
    [HttpGet("api/graph/nodes/{id}", Name = "GetNode")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UriNode>> GetNode(int id)
    {
        ItemGraphFactory factory = (await ItemIndexHelper
            .GetGraphFactoryAsync(_configuration, _serviceProvider))!;

        IGraphRepository repository = factory.GetGraphRepository()!;
        UriNode? node = repository.GetNode(id);
        if (node == null) return NotFound();
        return Ok(node);
    }

    /// <summary>
    /// Get the specified set of nodes.
    /// </summary>
    /// <param name="ids">The IDs of the nodes to get.</param>
    /// <returns>Node.</returns>
    [HttpGet("api/graph/nodes-set")]
    [ProducesResponseType(200)]
    public async Task<IList<UriNode>> GetNodeSet([FromQuery] IList<int> ids)
    {
        ItemGraphFactory factory = (await ItemIndexHelper
            .GetGraphFactoryAsync(_configuration, _serviceProvider))!;

        IGraphRepository repository = factory.GetGraphRepository()!;
        return repository.GetNodes(ids)!;
    }

    [HttpGet("api/graph/nodes-by-uri")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> GetNodeByUri([FromQuery] string uri)
    {
        ItemGraphFactory factory = (await ItemIndexHelper
            .GetGraphFactoryAsync(_configuration, _serviceProvider))!;

        IGraphRepository repository = factory.GetGraphRepository()!;
        UriNode? node = repository.GetNodeByUri(uri);
        if (node == null) return NotFound();
        return Ok(node);
    }

    [HttpGet("api/graph/walk/triples")]
    [ProducesResponseType(200)]
    public async Task<DataPage<TripleGroup>> GetTripleGroups([FromQuery]
        TripleFilterBindingModel model)
    {
        ItemGraphFactory factory = (await ItemIndexHelper
            .GetGraphFactoryAsync(_configuration, _serviceProvider))!;

        IGraphRepository repository = factory.GetGraphRepository()!;
        return repository.GetTripleGroups(
            model.ToTripleFilter(), model.Sort ?? "Cu");
    }

    [HttpGet("api/graph/walk/nodes")]
    [ProducesResponseType(200)]
    public async Task<DataPage<UriNode>> GetLinkedNodes([FromQuery]
        LinkedNodeFilterBindingModel model)
    {
        ItemGraphFactory factory = (await ItemIndexHelper
            .GetGraphFactoryAsync(_configuration, _serviceProvider))!;

        IGraphRepository repository = factory.GetGraphRepository()!;
        return repository.GetLinkedNodes(model.ToLinkedNodeFilter());
    }

    [HttpGet("api/graph/walk/nodes/literal")]
    [ProducesResponseType(200)]
    public async Task<DataPage<UriTriple>> GetLinkedLiterals([FromQuery]
        LinkedLiteralFilterBindingModel model)
    {
        ItemGraphFactory factory = (await ItemIndexHelper
            .GetGraphFactoryAsync(_configuration, _serviceProvider))!;

        IGraphRepository repository = factory.GetGraphRepository()!;
        return repository.GetLinkedLiterals(model.ToLinkedLiteralFilter());
    }

    /// <summary>
    /// Adds or updates the specified node.
    /// </summary>
    /// <param name="model">The node model.</param>
    [HttpPost("api/graph/nodes")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> AddNode([FromBody] NodeBindingModel model)
    {
        ItemGraphFactory factory = (await ItemIndexHelper
            .GetGraphFactoryAsync(_configuration, _serviceProvider))!;

        IGraphRepository repository = factory.GetGraphRepository()!;
        UriNode node = new()
        {
            Id = repository.AddUri(model.Uri!),
            IsClass = model.IsClass,
            Tag = model.Tag,
            Label = model.Label,
            SourceType = model.SourceType,
            Sid = model.Sid,
            Uri = model.Uri
        };
        repository.AddNode(node);
        return CreatedAtRoute("GetNode", new { node.Id }, node);
    }

    /// <summary>
    /// Deletes the node with the specified ID.
    /// </summary>
    /// <param name="id">The node's ID.</param>
    [HttpDelete("api/graph/nodes/{id}")]
    public async Task DeleteNode(int id)
    {
        ItemGraphFactory factory = (await ItemIndexHelper
            .GetGraphFactoryAsync(_configuration, _serviceProvider))!;

        IGraphRepository repository = factory.GetGraphRepository()!;
        repository.DeleteNode(id);
    }
    #endregion

    #region Triples
    /// <summary>
    /// Get the specified page of graph triples.
    /// </summary>
    /// <param name="model"></param>
    /// <returns>Page with triples.</returns>
    [HttpGet("api/graph/triples")]
    [ProducesResponseType(200)]
    public async Task<DataPage<UriTriple>> GetTriples([FromQuery]
        TripleFilterBindingModel model)
    {
        ItemGraphFactory factory = (await ItemIndexHelper
            .GetGraphFactoryAsync(_configuration, _serviceProvider))!;

        IGraphRepository repository = factory.GetGraphRepository()!;
        return repository.GetTriples(model.ToTripleFilter());
    }

    /// <summary>
    /// Get the triple with the specified ID.
    /// </summary>
    /// <param name="id">The triple ID.</param>
    /// <returns>Triple.</returns>
    [HttpGet("api/graph/triples/{id}", Name = "GetTriple")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UriNode>> GetTriple(int id)
    {
        ItemGraphFactory factory = (await ItemIndexHelper
            .GetGraphFactoryAsync(_configuration, _serviceProvider))!;

        IGraphRepository repository = factory.GetGraphRepository()!;
        UriTriple? triple = repository.GetTriple(id);
        if (triple == null) return NotFound();
        return Ok(triple);
    }

    /// <summary>
    /// Adds or updates the specified triple.
    /// </summary>
    /// <param name="model">The triple model.</param>
    [HttpPost("api/graph/triples")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> AddTriple(
        [FromBody] TripleBindingModel model)
    {
        ItemGraphFactory factory = (await ItemIndexHelper
            .GetGraphFactoryAsync(_configuration, _serviceProvider))!;

        IGraphRepository repository = factory.GetGraphRepository()!;

        Triple triple = new()
        {
            Id = model.Id,
            SubjectId = model.SubjectId,
            PredicateId = model.PredicateId,
            ObjectId = model.ObjectId,
            ObjectLiteral = model.ObjectLiteral,
            ObjectLiteralIx = model.ObjectLiteralIx,
            LiteralLanguage = model.LiteralLanguage,
            LiteralType = model.LiteralType,
            LiteralNumber = model.LiteralNumber,
            Sid = model.Sid,
            Tag = model.Tag
        };

        TripleObjectSupplier.Supply(triple, model.DefaultLanguage);

        repository.AddTriple(triple);
        return CreatedAtRoute("GetTriple", new { triple.Id }, triple);
    }

    /// <summary>
    /// Deletes the triple with the specified ID.
    /// </summary>
    /// <param name="id">The triple's ID.</param>
    [HttpDelete("api/graph/triples/{id}")]
    public async Task DeleteTriple(int id)
    {
        ItemGraphFactory factory = (await ItemIndexHelper
            .GetGraphFactoryAsync(_configuration, _serviceProvider))!;

        IGraphRepository repository = factory.GetGraphRepository()!;
        repository.DeleteTriple(id);
    }
    #endregion
}
