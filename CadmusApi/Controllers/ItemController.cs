using System;
using System.Linq;
using System.Text.RegularExpressions;
using Cadmus.Core.Blocks;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using CadmusApi.Models;
using CadmusApi.Services;
using Fusi.Tools.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CadmusApi.Controllers
{
    /// <summary>
    /// Items controller.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    // [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
    [ApiController]
    public sealed class ItemController : Controller
    {
        private static readonly Regex _pascalPropRegex = new Regex(@"""([A-Z])([^""]*)""\s*:");
        private static readonly Regex _camelPropRegex = new Regex(@"""([a-z])([^""]*)""\s*:");
        private static readonly Regex _guidRegex = new Regex(@"^[a-fA-F0-9]{32}$");
        private static readonly Regex _userIdRegex = new Regex(@"""userId""\s*:\s*""(?<v>[^""]+)""");

        private readonly RepositoryService _repositoryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemController" /> class.
        /// </summary>
        /// <param name="repositoryService">The repository service.</param>
        /// <exception cref="ArgumentNullException">repositoryService</exception>
        public ItemController(RepositoryService repositoryService)
        {
            _repositoryService = repositoryService ?? 
                throw new ArgumentNullException(nameof(repositoryService));
        }

        #region Get
        /// <summary>
        /// Gets the specified page of items.
        /// </summary>
        /// <param name="database">The name of the Mongo database.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>page</returns>
        [HttpGet("api/{database}/items")]
        [ProducesResponseType(200)]
        public IActionResult GetItems(string database, [FromQuery] ItemFilterModel filter)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            ICadmusRepository repository =
                _repositoryService.CreateRepository(database);
            PagedData<IItemInfo> page = repository.GetItemsPage(new ItemFilter
            {
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Title = filter.Title,
                Description = filter.Description,
                FacetId = filter.FacetId,
                Flags = filter.Flags,
                MinModified = filter.MinModified,
                MaxModified = filter.MaxModified,
                UserId = filter.UserId
            });
            return Ok(page);
        }

        /// <summary>
        /// Gets the item with the specified ID.
        /// </summary>
        /// <param name="database">The Mongo database name.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="parts">If set to <c>true</c>, include the parts in the returned
        /// item.</param>
        /// <returns>item</returns>
        [HttpGet("api/{database}/item/{id}", Name = "GetItem")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public IActionResult GetItem(string database, string id, [FromQuery] bool parts)
        {
            ICadmusRepository repository =
                _repositoryService.CreateRepository(database);
            IItem item = repository.GetItem(id, parts);
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
        /// <param name="database">The database.</param>
        /// <param name="id">The part's ID.</param>
        /// <returns>part</returns>
        [HttpGet("api/{database}/part/{id}", Name = "GetPart")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public IActionResult GetPart(string database, string id)
        {
            ICadmusRepository repository =
                _repositoryService.CreateRepository(database);
            string json = repository.GetPartJson(id);

            if (json == null) return new NotFoundResult();
            json = AdjustPartJson(json);

            object part = JsonConvert.DeserializeObject(json);
            return Ok(part);
        }

        /// <summary>
        /// From the item with the specified ID, gets the part matching the specified type
        /// and role.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="id">The item's ID.</param>
        /// <param name="type">The part type (e.g. "token-text").</param>
        /// <param name="role">The part role (use "default" for the default role).</param>
        /// <returns>part</returns>
        [HttpGet("api/{database}/item/{id}/part/{type}/{role}")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public IActionResult GetPartFromTypeAndRole(string database, string id, 
            string type, string role)
        {
            ICadmusRepository repository =
                _repositoryService.CreateRepository(database);

            IPart part = repository.GetItemParts(new[] {id}, type,
                    role == "default" ? null : role)
                .FirstOrDefault();
            if (part == null) return NotFound();

            string json = repository.GetPartJson(part.Id);
            if (json == null) return new NotFoundResult();
            json = AdjustPartJson(json);

            object result = JsonConvert.DeserializeObject(json);
            return Ok(result);
        }

        /// <summary>
        /// Gets the mappings between layer part role IDs and layer part IDs in the
        /// item with the specified ID.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="id">The item's identifier.</param>
        /// <returns>array where each item has roleId and partId</returns>
        [HttpGet("api/{database}/item/{id}/layers")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        public IActionResult GetItemLayerPartIds(string database, string id)
        {
            ICadmusRepository repository =
                _repositoryService.CreateRepository(database);

            var results = repository.GetItemLayerPartIds(id);
            return Ok(from t in results
                      select new
                      {
                          roleId = t.Item1,
                          partId = t.Item2
                      });
        }

        /// <summary>
        /// Gets the specified part's data pins.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="id">The part's identifier.</param>
        /// <returns>array of pins, each with <c>name</c> and <c>value</c></returns>
        [HttpGet("api/{database}/part/{id}/pins")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public IActionResult GetPartPins(string database, string id)
        {
            ICadmusRepository repository =
                _repositoryService.CreateRepository(database);
            string json = repository.GetPartJson(id);

            if (json == null) return new NotFoundResult();
            // remove ISODate(...) function (this seems to be a Mongo artifact)
            json = Regex.Replace(json, @"ISODate\(([^)]+)\)", "$1");
            // Pascal-case properties
            json = _camelPropRegex.Replace(json,
                m => $"\"{m.Groups[1].Value.ToUpperInvariant()}{m.Groups[2].Value}\":");

            Match typeMatch = Regex.Match(json, "\"TypeId\":\\s*\"([^\"]+)\"");
            if (!typeMatch.Success) return NotFound();

            Match roleMatch = Regex.Match(json, "\"RoleId\":\\s*\"([^\"]+)\"");
            string role = roleMatch.Success ? roleMatch.Groups[1].Value : null;

            IPartTypeProvider provider = _repositoryService.GetPartTypeProvider();
            Type t = provider.GetType(typeMatch.Groups[1].Value, role != null && 
                                                                 role.StartsWith("fr-"));
            IPart part = (IPart)JsonConvert.DeserializeObject(json, t);
            var result = (from p in part.GetDataPins()
                select new
                {
                    p.Name,
                    p.Value
                }).ToList();
            return Ok(result);
        }
        #endregion

        #region Delete
        /// <summary>
        /// Deletes the item with the specified ID.
        /// </summary>
        /// <param name="database">The database ID.</param>
        /// <param name="id">The item identifier.</param>
        [HttpDelete("api/{database}/item/{id}")]
        public void Delete(string database, string id)
        {
            ICadmusRepository repository =
                _repositoryService.CreateRepository(database);
            repository.DeleteItem(id, User.Identity.Name);
        }

        /// <summary>
        /// Deletes the part with the specified ID.
        /// </summary>
        /// <param name="database">The database ID.</param>
        /// <param name="id">The part identifier.</param>
        [HttpDelete("api/{database}/part/{id}")]
        public void DeletePart(string database, string id)
        {
            ICadmusRepository repository =
                _repositoryService.CreateRepository(database);
            repository.DeletePart(id, User.Identity.Name);
        }
        #endregion

        #region Post
        private static string SetUserId(string json, string value)
        {
            Match m = _userIdRegex.Match(json);
            if (!m.Success) return json;

            return json.Substring(0, m.Index) +
                   $"\"userId\":\"{value}\"" +
                   json.Substring(m.Index + m.Length);
        }

        /// <summary>
        /// Adds or updates the specified item.
        /// </summary>
        /// <param name="database">The database ID.</param>
        /// <param name="model">The item's model.</param>
        [HttpPost("api/{database}/items")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public IActionResult AddItem(string database, [FromBody] ItemBindingModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Item item = new Item
            {
                Title = model.Title,
                Description = model.Description,
                FacetId = model.FacetId,
                SortKey = model.SortKey,
                Flags = model.Flags,
                // override the user ID
                UserId = User.Identity.Name,
            };

            // set the item's ID if specified, else go with the default newly generated one
            if (!String.IsNullOrEmpty(model.Id) && _guidRegex.IsMatch(model.Id))
                item.Id = model.Id;

            ICadmusRepository repository =
                _repositoryService.CreateRepository(database);
            repository.AddItem(item);

            return CreatedAtRoute("GetItem", new
            {
                database,
                id = item.Id,
                parts = false
            }, item);
        }

        /// <summary>
        /// Adds or updates the specified part.
        /// </summary>
        /// <param name="database">The database ID.</param>
        /// <param name="model">The model with JSON code representing the part. If new, the part ID
        /// should not be parsable as a GUID, e.g. <c>"id": "new"</c> or <c>"id": ""</c>,
        /// or should be null (e.g. <c>"id": null</c>). At a minimum, each part should 
        /// adhere to this model: <c>{ "id" : "32-chars-GUID-value", "_t" : "C#-part-name", 
        /// "itemId" : "32-chars-GUID-value", "typeId" : "type-id", 
        /// "roleId" : null or "role-id", "timeModified" : "ISO date and time"), 
        /// "userId" : "user-id or empty" }</c>.</param>
        [HttpPost("api/{database}/parts")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public IActionResult AddPart(string database, [FromBody] RawJsonBindingModel model)
        {
            ICadmusRepository repository =
                _repositoryService.CreateRepository(database);

            // add the ID if new part
            string json = model.Raw;
            string partId;
            Match m = Regex.Match(json,
                @"""_id""\s*:\s*(?:(?:""(?<v>[^""]*)"")|(?<v>:null))");
            if (!m.Success)
                return BadRequest("Invalid part JSON: missing id");

            bool isNew = !_guidRegex.IsMatch(m.Groups["v"].Value);
            if (isNew)
            {
                partId = Guid.NewGuid().ToString("N");
                json = json.Substring(0, m.Index) +
                       $"\"_id\":\"{partId}\"" +
                       json.Substring(m.Index + m.Length);
            }
            else partId = m.Groups["v"].Value;

            // override the user ID
            json = SetUserId(json, User.Identity.Name ?? "");

            // add the part
            repository.AddPartJson(json);
            return CreatedAtRoute("GetPart", new
            {
                database,
                id = partId
            }, json);
        }
        #endregion
    }
}
