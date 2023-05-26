using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cadmus.Api.Services.Auth;
using Cadmus.Api.Services.Seeding;
using Cadmus.Core;
using Cadmus.Core.Config;
using Cadmus.Core.Layers;
using Cadmus.Core.Storage;
using Cadmus.Index;
using Cadmus.Index.Config;
using Cadmus.Api.Models;
using Fusi.Tools.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Cadmus.Graph;
using Microsoft.Extensions.Caching.Memory;

namespace Cadmus.Api.Controllers;

/// <summary>
/// Items controller.
/// </summary>
[Authorize]
[ApiController]
public sealed class ItemController : Controller
{
    private static readonly Regex _pascalPropRegex =
        new(@"""([A-Z])([^""]*)""\s*:", RegexOptions.Compiled);

    private static readonly Regex _camelPropRegex =
        new(@"""([a-z])([^""]*)""\s*:", RegexOptions.Compiled);

    private static readonly Regex _guidRegex =
        new("^[a-fA-F0-9]{8}-" +
            "[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}$",
            RegexOptions.Compiled);

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRepositoryProvider _repositoryProvider;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemController" /> class.
    /// </summary>
    /// <param name="userManager">The user manager.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="repositoryProvider">The repository provider.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">repositoryService</exception>
    public ItemController(UserManager<ApplicationUser> userManager,
        IServiceProvider serviceProvider,
        IRepositoryProvider repositoryProvider,
        IConfiguration configuration,
        IMemoryCache cache,
        ILogger logger)
    {
        _userManager = userManager ??
            throw new ArgumentNullException(nameof(userManager));
        _serviceProvider = serviceProvider ??
            throw new ArgumentNullException(nameof(serviceProvider));
        _repositoryProvider = repositoryProvider ??
            throw new ArgumentNullException(nameof(repositoryProvider));
        _configuration = configuration ??
            throw new ArgumentNullException(nameof(configuration));
        _cache = cache ??
            throw new ArgumentNullException(nameof(cache));
        _logger = logger ??
            throw new ArgumentNullException(nameof(logger));
    }

    private bool IsIndexingEnabled() =>
        _configuration.GetValue<bool>("Indexing:IsEnabled");

    private bool IsGraphEnabled() =>
        _configuration.GetValue<bool>("Indexing:IsGraphEnabled");

    #region Get
    /// <summary>
    /// Gets the specified page of items.
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <returns>page</returns>
    [HttpGet("api/items")]
    [ProducesResponseType(200)]
    public ActionResult<DataPage<ItemInfo>> GetItems(
        [FromQuery] ItemFilterModel filter)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();
        DataPage<ItemInfo> page = repository.GetItems(new ItemFilter
        {
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            Title = filter.Title,
            Description = filter.Description,
            FacetId = filter.FacetId,
            GroupId = filter.GroupId,
            Flags = filter.Flags,
            FlagMatching = filter.FlagMatching,
            MinModified = filter.MinModified,
            MaxModified = filter.MaxModified,
            UserId = filter.UserId
        });
        return Ok(page);
    }

    /// <summary>
    /// Gets the item with the specified ID.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="parts">If set to <c>true</c>, include the parts in the
    /// returned item.</param>
    /// <returns>item</returns>
    [HttpGet("api/items/{id}", Name = "GetItem")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public ActionResult<IItem> GetItem(
        [FromRoute] string id,
        [FromQuery] bool parts)
    {
        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();
        IItem? item = repository.GetItem(id, parts);
        if (item == null) return new NotFoundResult();
        return Ok(item);
    }

    private static string AdjustPartJson(string json)
    {
        // remove ISODate(...) function (this seems to be a Mongo artifact)
        json = Regex.Replace(json, @"ISODate\(([^)]+)\)", "$1");

        // camel-case properties
        json = _pascalPropRegex.Replace(json,
            m => $"\"{m.Groups[1].Value.ToLowerInvariant()}{m.Groups[2].Value}\":");

        // replace _id with id
        json = json.Replace("\"_id\":", "\"id\":");

        return json;
    }

    /// <summary>
    /// Gets the part with the specified ID.
    /// </summary>
    /// <param name="id">The part's ID.</param>
    /// <returns>part</returns>
    [HttpGet("api/parts/{id}", Name = "GetPart")]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public IActionResult GetPart(
        [FromRoute] string id)
    {
        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();
        string? json = repository.GetPartContent(id);

        if (json == null) return new NotFoundResult();
        json = AdjustPartJson(json);

        object part = JsonConvert.DeserializeObject(json)!;
        return Ok(part);
    }

    /// <summary>
    /// From the item with the specified ID, gets the first part matching
    /// the specified type and/or role. You should use this method to
    /// find a part which by its definition (from its type and/or role ID)
    /// is unique. If more than 1 parts match in the item, which of them
    /// will be returned is undetermined.
    /// </summary>
    /// <param name="id">The item's ID.</param>
    /// <param name="type">The part type ID, or <c>any</c> to match any
    /// part type. This is typically used when requesting the <c>base-text</c>
    /// role.</param>
    /// <param name="role">The part role (use <c>default</c> for the default
    /// role).</param>
    /// <returns>part</returns>
    [HttpGet("api/items/{id}/parts/{type}/{role}")]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public IActionResult GetPartFromTypeAndRole(
        [FromRoute] string id,
        [FromRoute] string type,
        [FromRoute] string role)
    {
        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();

        IPart? part = repository.GetItemParts(new[] { id },
            type == "any" ? null : type,
            role == "default" ? null : role)
            .FirstOrDefault();
        if (part == null) return NotFound();

        string? json = repository.GetPartContent(part.Id);
        if (json == null) return new NotFoundResult();
        json = AdjustPartJson(json);

        object result = JsonConvert.DeserializeObject(json)!;
        return Ok(result);
    }

    /// <summary>
    /// Gets the data pin definitions for the specified type ID.
    /// </summary>
    /// <param name="typeId">The type identifier: this is the type ID
    /// of the part or fragment, e.g. <c>it.vedph.token-text</c> or
    /// <c>fr.it.vedph.comment</c>.</param>
    /// <returns>Definitions, empty if type ID not found or no definitions
    /// were present. The <c>type</c> property in the definition is
    /// 0=string, 1=boolean, 2=integer, 3=decimal.</returns>
    [HttpGet("api/pin-defs/{typeId}")]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    public DataPinDefinition[] GetDataPinDefinitions([FromRoute] string typeId)
    {
        IPartTypeProvider provider = _repositoryProvider.GetPartTypeProvider();
        Type t = provider.Get(typeId)!;
        if (typeof(IHasDataPins).IsAssignableFrom(t))
        {
            IHasDataPins? p = Activator.CreateInstance(t) as IHasDataPins;
            return p?.GetDataPinDefinitions()?.ToArray()
                ?? Array.Empty<DataPinDefinition>();
        }
        return Array.Empty<DataPinDefinition>();
    }

    /// <summary>
    /// Gets the base text part and its plain text for the item with the
    /// specified ID.
    /// </summary>
    /// <param name="id">The item identifier.</param>
    /// <returns>Object with <c>Part</c> and <c>Text</c> property, both
    /// null if no base text part found for the item.</returns>
    [HttpGet("api/items/{id}/base-text")]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    public IActionResult GetBaseText([FromRoute] string id)
    {
        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();

        IHasText? partWithText = repository.GetItemParts(
            new[] { id },
            null,
            PartBase.BASE_TEXT_ROLE_ID)
            .FirstOrDefault() as IHasText;
        return Ok(new
        {
            Part = partWithText,
            Text = partWithText?.GetText()
        });
    }

    /// <summary>
    /// Gets the information about all the layer parts in the item with
    /// the specified ID. If <paramref name="absent"/> is true, the
    /// layer parts are added also from the item's facet, even if absent
    /// from the database. This produces the full list of all the possible
    /// layer parts which could be connected to the given item.
    /// </summary>
    /// <param name="id">The identifier or <c>new</c> for a new item.</param>
    /// <param name="absent">if set to <c>true</c> [absent].</param>
    /// <returns>List of layer part infos.</returns>
    [HttpGet("api/items/{id}/layers")]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    public IActionResult GetItemLayerInfo(
        [FromRoute] string id,
        [FromQuery] bool absent)
    {
        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();

        IList<LayerPartInfo> parts = repository.GetItemLayerInfo(id, absent);
        return Ok(parts);
    }

    /// <summary>
    /// Gets the specified part's data pins.
    /// </summary>
    /// <param name="id">The part's identifier.</param>
    /// <returns>array of pins, each with <c>name</c> and <c>value</c>
    /// </returns>
    [HttpGet("api/parts/{id}/pins")]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public IActionResult GetPartPins(
        [FromRoute] string id)
    {
        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();
        string? json = repository.GetPartContent(id);

        if (json == null) return new NotFoundResult();
        // remove ISODate(...) function (this seems to be a Mongo artifact)
        json = Regex.Replace(json, @"ISODate\(([^)]+)\)", "$1");
        // Pascal-case properties
        json = _camelPropRegex.Replace(json,
            m => $"\"{m.Groups[1].Value.ToUpperInvariant()}{m.Groups[2].Value}\":");

        Match typeMatch = Regex.Match(json, "\"TypeId\":\\s*\"([^\"]+)\"");
        if (!typeMatch.Success) return NotFound();

        IPartTypeProvider provider = _repositoryProvider.GetPartTypeProvider();
        Type t = provider.Get(typeMatch.Groups[1].Value)!;
        IPart part = (IPart)JsonConvert.DeserializeObject(json, t)!;
        var result = (from p in part.GetDataPins()
                      select new
                      {
                          p.Name,
                          p.Value
                      }).ToList();
        return Ok(result);
    }

    /// <summary>
    /// Gets the layer part break chance, a number indicating whether the
    /// layer part with the specified ID might potentially be broken
    /// because of changes in its base text.
    /// </summary>
    /// <param name="id">The layer part's identifier.</param>
    /// <returns>Object with <c>chance</c>=0 (=not broken), 1 (=potentially
    /// broken), or 2 (=surely broken).</returns>
    [HttpGet("api/parts/{id}/break-chance")]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    public IActionResult GetLayerPartBreakChance(
        [FromRoute] string id)
    {
        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();

        string? intervalOption = _configuration.GetSection("Editing")
            ["BaseToLayerToleranceSeconds"];
        int interval = !string.IsNullOrEmpty(intervalOption)
            && int.TryParse(intervalOption, out int n)
            ? n
            : 60;
        int chance = repository.GetLayerPartBreakChance(id, interval);
        return Ok(new
        {
            Chance = chance
        });
    }

    /// <summary>
    /// Gets the reconciliation hints for the layer part with the specified
    /// ID.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns>List of hints.</returns>
    [HttpGet("api/parts/{id}/layer-hints")]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    public ActionResult<LayerHint> GetLayerPartHints(
        [FromRoute] string id)
    {
        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();

        return Ok(repository.GetLayerPartHints(id));
    }

    /// <summary>
    /// Applies patches operations to the layer part with the specified
    /// ID.
    /// </summary>
    /// <param name="id">The layer part identifier.</param>
    /// <param name="patches">The patches to be applied.</param>
    [Authorize(Roles = "admin,editor,operator")]
    [HttpPost("api/parts/{id}/layer-patches")]
    [Produces("application/json")]
    [ProducesResponseType(201)]
    public IActionResult ApplyLayerPartPatches(
        [FromRoute] string id,
        [FromBody] string[] patches)
    {
        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();

        string json = repository.ApplyLayerPartPatches(
            id, User.Identity!.Name!, patches)!;

        return CreatedAtRoute("GetPart", new
        {
            id
        }, json);
    }
    #endregion

    #region Delete
    /// <summary>
    /// Deletes the item with the specified ID.
    /// This requires <c>admin</c> or <c>editor</c> role.
    /// </summary>
    /// <param name="id">The item identifier.</param>
    [Authorize(Roles = "admin,editor")]
    [HttpDelete("api/items/{id}")]
    public async Task Delete([FromRoute] string id)
    {
        _logger.Information("User {UserName} deleting item {ItemId} from {IP}",
            User.Identity!.Name,
            id,
            HttpContext.Connection.RemoteIpAddress);
        ICadmusRepository repository = _repositoryProvider.CreateRepository();
        bool isGraphEnabled = IsGraphEnabled();

        // if we're going to update the graph, collect part IDs
        List<string> partIds = new();
        if (isGraphEnabled)
        {
            foreach (IPart part in repository.GetItemParts(new[] { id }))
                partIds.Add(part.Id);
        }

        // delete item and parts
        repository.DeleteItem(id, User.Identity!.Name!);

        // update index if required
        if (IsIndexingEnabled())
        {
            ItemIndexFactory factory = (await ItemIndexHelper
                .GetIndexFactoryAsync(_configuration, _serviceProvider))!;
            IItemIndexWriter writer = factory.GetItemIndexWriter()!;
            await writer.DeleteItem(id);
            writer.Close();

            if (isGraphEnabled)
            {
                foreach (string partId in partIds) UpdateGraphForDeletion(partId);
                UpdateGraphForDeletion(id);
            }
        }
    }

    private async Task<bool> IsUserInRole(ApplicationUser user, string role,
        HashSet<string> excludedRoles)
    {
        IList<string> userRoles = await _userManager.GetRolesAsync(user);
        return userRoles.Contains(role)
               && userRoles.All(r => !excludedRoles.Contains(r));
    }

    /// <summary>
    /// Deletes the part with the specified ID.
    /// This requires <c>admin</c> or <c>editor</c> role; a user with
    /// the <c>operator</c> role can delete only the parts created by
    /// himself.
    /// </summary>
    /// <param name="id">The part identifier.</param>
    [Authorize(Roles = "admin,editor,operator")]
    [HttpDelete("api/parts/{id}")]
    public async Task<IActionResult> DeletePart([FromRoute] string id)
    {
        _logger.Information("User {UserName} deleting part {PartId} from {IP}",
            User.Identity!.Name!,
            id,
            HttpContext.Connection.RemoteIpAddress);

        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();

        // operators can delete only parts created by themselves
        ApplicationUser user = (await _userManager.FindByNameAsync(
            User.Identity!.Name!))!;
        if (await IsUserInRole(user,
                "operator",
                new HashSet<string>(new string[] { "admin", "editor" }))
            && repository.GetPartCreatorId(id) != user.UserName)
        {
            return Unauthorized();
        }

        repository.DeletePart(id, User.Identity!.Name!);

        // update index if required
        if (IsIndexingEnabled())
        {
            bool isGraphEnabled = IsGraphEnabled();

            ItemIndexFactory factory = (await ItemIndexHelper
                .GetIndexFactoryAsync(_configuration, _serviceProvider))!;
            IItemIndexWriter writer = factory.GetItemIndexWriter()!;
            await writer.DeletePart(id);
            writer.Close();

            if (isGraphEnabled) UpdateGraphForDeletion(id);
        }

        return Ok();
    }
    #endregion

    #region Post
    private IGraphRepository? GetGraphRepository()
    {
        IGraphRepository? graphRepository =
            _serviceProvider.GetService<IGraphRepository>();
        if (graphRepository == null)
        {
            _logger?.Error("Unable to get IGraphRepository service");
            return null;
        }
        graphRepository.Cache = _cache;
        return graphRepository;
    }

    private void UpdateGraph(IItem item)
    {
        IGraphRepository? graphRepository = GetGraphRepository();
        if (graphRepository == null) return;

        _logger.Information("Updating graph for item " + item);
        GraphUpdater updater = _serviceProvider.GetService<GraphUpdater>()
            ?? new GraphUpdater(graphRepository);
        updater.Update(item);
    }

    private void UpdateGraph(IItem item, IPart part)
    {
        IGraphRepository? graphRepository = GetGraphRepository();
        if (graphRepository == null) return;

        _logger.Information("Updating graph for part " + part);
        GraphUpdater updater = _serviceProvider.GetService<GraphUpdater>()
            ?? new GraphUpdater(graphRepository);
        updater.Update(item, part);
    }

    private void UpdateGraphForDeletion(string id)
    {
        IGraphRepository? graphRepository = GetGraphRepository();
        if (graphRepository == null) return;

        _logger.Information("Updating graph for deleted " + id);
        graphRepository.DeleteGraphSet(id);
    }

    /// <summary>
    /// Adds or updates the specified item.
    /// This requires <c>admin</c>, <c>editor</c>, or <c>operator</c> role.
    /// </summary>
    /// <param name="model">The item's model.</param>
    [Authorize(Roles = "admin,editor,operator")]
    [HttpPost("api/items")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AddItem([FromBody] ItemBindingModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        Item item = new()
        {
            Title = model.Title!,
            Description = model.Description!,
            FacetId = model.FacetId!,
            GroupId = model.GroupId,
            SortKey = model.SortKey!,
            Flags = model.Flags,
            // override the user ID and set the creator ID,
            // which anyway will be ignored if already set
            UserId = User.Identity!.Name!,
            CreatorId = User.Identity!.Name!
        };

        // set the item's ID if specified, else go with the default
        // newly generated one
        if (!string.IsNullOrEmpty(model.Id) && _guidRegex.IsMatch(model.Id))
            item.Id = model.Id;

        _logger.Information("User {UserName} saving item {ItemId} from {IP}",
            User.Identity.Name,
            item.Id,
            HttpContext.Connection.RemoteIpAddress);

        ICadmusRepository repository = _repositoryProvider.CreateRepository();
        repository.AddItem(item);

        // update index
        if (IsIndexingEnabled())
        {
            bool isGraphEnabled = IsGraphEnabled();
            ItemIndexFactory factory = (await ItemIndexHelper
                .GetIndexFactoryAsync(_configuration, _serviceProvider))!;
            IItemIndexWriter writer = factory.GetItemIndexWriter()!;
            await writer.WriteItem(item);
            writer.Close();

            // graph
            if (isGraphEnabled) UpdateGraph(item);
        }

        return CreatedAtRoute("GetItem", new
        {
            id = item.Id,
            parts = false
        }, item);
    }

    /// <summary>
    /// Sets the flags value for all the items with the specified IDs.
    /// </summary>
    /// <param name="model">The model.</param>
    [Authorize(Roles = "admin,editor,operator")]
    [HttpPost("api/items/flags")]
    [ProducesResponseType(200)]
    public IActionResult SetItemFlags(
        [FromBody] ItemFlagsBindingModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();

        if (model.Ids != null)
            repository.SetItemFlags(model.Ids, model.Flags);
        return Ok();
    }

    /// <summary>
    /// Sets the group ID for all the items with the specified IDs.
    /// </summary>
    /// <param name="model">The model.</param>
    [Authorize(Roles = "admin,editor,operator")]
    [HttpPost("api/items/group-id")]
    [ProducesResponseType(200)]
    public IActionResult SetItemGroupId(
        [FromBody] ItemGroupIdBindingModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();

        if (model.Ids != null)
            repository.SetItemGroupId(model.Ids, model.GroupId);
        return Ok();
    }

    private static bool IsNewPart(JObject part)
    {
        if (!part.ContainsKey("id")) return true;

        JValue id = (JValue)part["id"]!;
        return id == null
            || id.Type == JTokenType.Null
            || !_guidRegex.IsMatch(id.Value<string>()!);
    }

    private Tuple<string, string, string> AddRawPart(string raw)
    {
        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();

        JObject doc = JObject.Parse(raw);

        // add the ID if new part
        string partId;
        bool isNew = IsNewPart(doc);
        if (isNew)
        {
            partId = Guid.NewGuid().ToString();
            if (doc.ContainsKey("id")) doc.Property("id")!.Remove();
            doc.AddFirst(new JProperty("id", partId));
        }
        else
        {
            JValue id = (JValue)doc["id"]!;
            partId = id.Value<string>()!;
        }

        // set the creator ID if not specified
        JValue creatorId = (JValue)doc["creatorId"]!;
        if (creatorId == null
            || creatorId.Type == JTokenType.Null
            || string.IsNullOrEmpty(creatorId?.Value<string>()))
        {
            if (doc.ContainsKey("creatorId"))
                doc.Property("creatorId")!.Remove();
            doc.Add(new JProperty("creatorId", User.Identity!.Name));
        }

        // override the creation time if new
        if (isNew)
        {
            if (doc.ContainsKey("timeCreated"))
                doc.Property("timeCreated")!.Remove();
            doc.Add(new JProperty("timeCreated", DateTime.UtcNow));
        }

        // override the user ID
        if (doc.ContainsKey("userId")) doc.Property("userId")!.Remove();
        doc.Add(new JProperty("userId", User.Identity!.Name ?? ""));

        // get the item's ID
        JValue itemId = (JValue)doc["itemId"]!;

        // add the part
        _logger.Information("User {UserName} saving part {PartId} from {IP}",
            User.Identity.Name,
            partId,
            HttpContext.Connection.RemoteIpAddress);

        string json = doc.ToString(Formatting.None);
        repository.AddPartFromContent(json);
        return Tuple.Create(itemId.Value<string>()!, partId, json);
    }

    /// <summary>
    /// Adds or updates the specified part.
    /// This requires <c>admin</c>, <c>editor</c>, or <c>operator</c> role.
    /// </summary>
    /// <param name="model">The model with JSON code representing the part.
    /// If new, the part ID should not be parsable as a GUID, e.g.
    /// <c>"id": "new"</c> or <c>"id": ""</c>, or should be null
    /// (e.g. <c>"id": null</c>). At a minimum, each part should adhere
    /// to this model: <code>{ "id" : "32-chars-GUID-value", "_t" : "C#-part-name",
    /// "itemId" : "32-chars-GUID-value", "typeId" : "type-id",
    /// "roleId" : null or "role-id", "timeModified" : "ISO date and time"),
    /// "userId" : "user-id or empty" }</code>.</param>
    [Authorize(Roles = "admin,editor,operator")]
    [HttpPost("api/parts")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AddPart(
        [FromBody] RawJsonBindingModel model)
    {
        var idAndJson = AddRawPart(model.Raw!);

        // update index if required
        if (IsIndexingEnabled())
        {
            bool isGraphEnabled = IsGraphEnabled();

            ICadmusRepository repository =
                _repositoryProvider.CreateRepository();
            ItemIndexFactory factory = (await ItemIndexHelper
                .GetIndexFactoryAsync(_configuration, _serviceProvider))!;
            IItemIndexWriter writer = factory.GetItemIndexWriter()!;

            IPart? part = repository.GetPart<IPart>(idAndJson.Item2);
            if (part != null)
            {
                IItem? item = repository.GetItem(idAndJson.Item1);
                if (item != null)
                {
                    await writer.WritePart(item, part);
                }
                writer.Close();

                // graph
                if (isGraphEnabled)
                {
                    item = repository.GetItem(part.ItemId);
                    if (item != null) UpdateGraph(item, part);
                }
            }
        }

        return CreatedAtRoute("GetPart", new
        {
            id = idAndJson.Item2
        }, idAndJson.Item3);
    }

    /// <summary>
    /// Sets the thesaurus scope for all the parts with the specified IDs.
    /// </summary>
    /// <param name="model">The model.</param>
    [Authorize(Roles = "admin,editor,operator")]
    [HttpPost("api/parts/thes-scope")]
    [ProducesResponseType(200)]
    public IActionResult SetPartThesaurusScope(
        [FromBody] PartThesaurusScopeBindingModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();

        repository.SetPartThesaurusScope(model.Ids!, model.Scope ?? "");
        return Ok();
    }
    #endregion
}
