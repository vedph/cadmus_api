using Cadmus.Api.Models.Preview;
using Cadmus.Export;
using Cadmus.Export.Preview;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Cadmus.Api.Controllers;

/// <summary>
/// Preview controller.
/// </summary>
/// <seealso cref="Controller" />
[Authorize]
[ApiController]
public sealed class PreviewController : ControllerBase
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
    /// <param name="type">The type of component key to get: <c>F</c>
    /// =text flatteners, <c>C</c>=item composers, <c>J</c>=JSON renderers.
    /// </param>
    /// <returns>List of unique keys.</returns>
    [HttpGet("api/preview/keys")]
    [ProducesResponseType(200)]
    public HashSet<string> GetKeys([FromQuery] char type)
    {
        if (!IsPreviewEnabled()) return new HashSet<string>();
        return char.ToUpperInvariant(type) switch
        {
            'F' => _previewer.GetFlattenerKeys(),
            'C' => _previewer.GetComposerKeys(),
            _ => _previewer.GetJsonRendererKeys(),
        };
    }

    /// <summary>
    /// Renders the part with the specified ID.
    /// </summary>
    /// <param name="itemId">The item's identifier.</param>
    /// <param name="partId">The part's identifier.</param>
    /// <returns>Rendition or empty string.</returns>
    [HttpGet("api/preview/items/{itemId}/parts/{partId}")]
    [ProducesResponseType(200)]
    public RenditionModel RenderPart([FromRoute] string itemId,
        [FromRoute] string partId)
    {
        if (!IsPreviewEnabled()) return new RenditionModel("");
        return new RenditionModel(_previewer.RenderPart(itemId, partId));
    }

    /// <summary>
    /// Renders the fragment at index <paramref name="frIndex"/> in the layer
    /// part with ID equal to <paramref name="partId"/>.
    /// </summary>
    /// <param name="itemId">The item's identifier.</param>
    /// <param name="partId">The part's identifier.</param>
    /// <param name="frIndex">The index of the fragment in the part (0-N).
    /// </param>
    /// <returns>Rendition or empty string.</returns>
    [HttpGet("api/preview/items/{itemId}/parts/{partId}/{frIndex}")]
    [ProducesResponseType(200)]
    public RenditionModel RenderFragment([FromRoute] string itemId,
        [FromRoute] string partId,
        [FromRoute] int frIndex)
    {
        if (!IsPreviewEnabled()) return new RenditionModel("");
        return new RenditionModel(_previewer.RenderFragment(
            itemId, partId, frIndex));
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
        List<string?> lids = new();
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
