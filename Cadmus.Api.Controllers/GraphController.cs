using Cadmus.Api.Models;
using Cadmus.Api.Services.Seeding;
using Cadmus.Index.Config;
using Cadmus.Index.Graph;
using Fusi.Tools.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Cadmus.Api.Controllers
{
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

        /// <summary>
        /// Get the specified page of graph nodes.
        /// </summary>
        /// <param name="model">The filter.</param>
        /// <returns>A page of nodes.</returns>
        [HttpGet("api/graph/nodes")]
        [ProducesResponseType(200)]
        public async Task<DataPage<NodeResult>> GetNodes([FromQuery]
            NodeFilterBindingModel model)
        {
            ItemIndexFactory factory = await ItemIndexHelper
                .GetIndexFactoryAsync(_configuration, _serviceProvider);

            IGraphRepository repository = factory.GetGraphRepository();
            return repository.GetNodes(model.ToNodeFilter());
        }

        /// <summary>
        /// Get the node with the specified ID.
        /// </summary>
        /// <param name="id">The node ID.</param>
        /// <returns>Node.</returns>
        [HttpGet("api/graph/nodes/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<NodeResult>> GetNode(int id)
        {
            ItemIndexFactory factory = await ItemIndexHelper
                .GetIndexFactoryAsync(_configuration, _serviceProvider);

            IGraphRepository repository = factory.GetGraphRepository();
            NodeResult node = repository.GetNode(id);
            if (node == null) return NotFound();
            return Ok(node);
        }

        /// <summary>
        /// Get the specified page of graph triples.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet("api/graph/triples")]
        [ProducesResponseType(200)]
        public async Task<DataPage<TripleResult>> GetTriples([FromQuery]
            TripleFilterBindingModel model)
        {
            ItemIndexFactory factory = await ItemIndexHelper
                .GetIndexFactoryAsync(_configuration, _serviceProvider);

            IGraphRepository repository = factory.GetGraphRepository();
            return repository.GetTriples(model.ToTripleFilter());
        }

        /// <summary>
        /// Get the node with the specified ID.
        /// </summary>
        /// <param name="id">The node ID.</param>
        /// <returns>Node.</returns>
        [HttpGet("api/graph/triples/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<NodeResult>> GetTriple(int id)
        {
            ItemIndexFactory factory = await ItemIndexHelper
                .GetIndexFactoryAsync(_configuration, _serviceProvider);

            IGraphRepository repository = factory.GetGraphRepository();
            TripleResult triple = repository.GetTriple(id);
            if (triple == null) return NotFound();
            return Ok(triple);
        }
    }
}
