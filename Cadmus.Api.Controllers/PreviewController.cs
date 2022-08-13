using Cadmus.Api.Models.Preview;
using Cadmus.Export;
using Cadmus.Export.Preview;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Cadmus.Api.Controllers
{
    /// <summary>
    /// Preview controller.
    /// </summary>
    /// <seealso cref="Controller" />
    [Authorize]
    [ApiController]
    public sealed class PreviewController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly CadmusPreviewer _previewer;

        private bool IsPreviewEnabled() =>
            _previewer != null &&
            _configuration.GetValue<bool>("Preview:IsEnabled");

        public PreviewController(IConfiguration configuration,
            CadmusPreviewer previewer)
        {
            _configuration = configuration;
            _previewer = previewer;
        }

        /// <summary>
        /// Gets all the Cadmus objects keys registered for preview.
        /// </summary>
        /// <param name="flatteners">if set to <c>true</c>, get keys for
        /// text flatteners. If <c>false</c>, get keys for JSON renderers.
        /// </param>
        /// <returns>List of unique keys.</returns>
        [HttpGet("api/preview/keys")]
        [ProducesResponseType(200)]
        public HashSet<string> GetKeys([FromQuery] bool flatteners)
        {
            if (!IsPreviewEnabled()) return new HashSet<string>();
            return _previewer.GetKeys(flatteners);
        }

        /// <summary>
        /// Renders the part with the specified ID.
        /// </summary>
        /// <param name="id">The part's identifier.</param>
        /// <returns>Rendition or empty string.</returns>
        [HttpGet("api/preview/parts/{id}")]
        [ProducesResponseType(200)]
        public RenditionModel RenderPart([FromRoute] string id)
        {
            if (!IsPreviewEnabled()) return new RenditionModel("");
            return new RenditionModel(_previewer.RenderPart(id));
        }

        /// <summary>
        /// Renders the fragment at index <paramref name="frIndex"/> in the layer
        /// part with ID equal to <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The part's identifier.</param>
        /// <param name="frIndex">The index of the fragment in the part (0-N).
        /// </param>
        /// <returns>Rendition or empty string.</returns>
        [HttpGet("api/preview/parts/{id}/{frIndex}")]
        [ProducesResponseType(200)]
        public RenditionModel RenderFragment([FromRoute] string id,
            [FromRoute] int frIndex)
        {
            if (!IsPreviewEnabled()) return new RenditionModel("");
            return new RenditionModel(_previewer.RenderFragment(id, frIndex));
        }

        /// <summary>
        /// Gets the text blocks built by flattening the text part with the
        /// specified ID with all the layers specified.
        /// </summary>
        /// <param name="id">The base text part's identifier.</param>
        /// <param name="layerId">The layer parts identifiers. Optionally,
        /// each identifier can be suffixed with <c>=</c> followed by
        /// an arbitrary ID to assign to the layer type of that part.</param>
        /// <returns>List of block rows.</returns>
        [HttpGet("api/preview/text-parts/{id}")]
        [ProducesResponseType(200)]
        public IList<TextBlockRow> GetTextBlocks([FromRoute] string id,
            [FromQuery] string[] layerId)
        {
            if (!IsPreviewEnabled()) return Array.Empty<TextBlockRow>();

            List<string> pids = new();
            List<string> lids = new();
            for (int i = 0; i < layerId.Length; i++)
            {
                int j = layerId[i].IndexOf('=');
                if (j == -1)
                {
                    pids.Add(layerId[i]);
                    lids.Add(null);
                }
                else
                {
                    pids.Add(layerId[i][0..j]);
                    lids.Add(layerId[i][(j + 1)..]);
                }
            }

            return _previewer.BuildTextBlocks(id, pids, lids);
        }
    }
}
