using System;
using System.Linq;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using CadmusApi.Models;
using CadmusApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CadmusApi.Controllers
{
    /// <summary>
    /// Tags controller.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    // [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
    [ApiController]
    public sealed class TagController : Controller
    {
        private readonly RepositoryService _repositoryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagController"/> class.
        /// </summary>
        /// <param name="repositoryService">The repository service.</param>
        /// <exception cref="ArgumentNullException">repository</exception>
        public TagController(RepositoryService repositoryService)
        {
            _repositoryService = repositoryService ??
                                 throw new ArgumentNullException(nameof(repositoryService));
        }

        /// <summary>
        /// Gets the list of all the tag sets IDs.
        /// </summary>
        /// <param name="database">The name of the Mongo database.</param>
        /// <returns>list of tag sets IDs</returns>
        [HttpGet("api/{database}/tags")]
        public IActionResult GetSetIds(string database)
        {
            ICadmusRepository repository = _repositoryService.CreateRepository(database);
            return Ok(repository.GetTagSetIds());
        }

        /// <summary>
        /// Gets the tags set with the specified ID.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="id">The tags set ID.</param>
        /// <returns>set</returns>
        [HttpGet("api/{database}/tag/{id}", Name = "GetSet")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public IActionResult GetSet(string database, string id)
        {
            ICadmusRepository repository = _repositoryService.CreateRepository(database);
            TagSet set = repository.GetTagSet(id);
            if (set == null) return NotFound();
            return Ok(set);
        }

        /// <summary>
        /// Adds or updates the specified tags set.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="model">The tags set model.</param>
        [HttpPost("api/{database}/tags")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public IActionResult AddSet(string database, [FromBody] TagSetBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            ICadmusRepository repository = _repositoryService.CreateRepository(database);
            TagSet set = new TagSet
            {
                Id = model.Id,
                Tags = (from m in model.Tags
                        select new Tag { Id = m.Id, Name = m.Name }).ToList()
            };
            repository.AddTagSet(set);

            return CreatedAtRoute("GetSet", new
            {
                database,
                id = set.Id
            }, set);
        }

        /// <summary>
        /// Deletes the tags set with the specified ID.
        /// </summary>
        /// <param name="database">The database ID.</param>
        /// <param name="id">The tags set ID.</param>
        [HttpDelete("api/{database}/tag/{id}")]
        public void DeleteSet(string database, string id)
        {
            ICadmusRepository repository =
                _repositoryService.CreateRepository(database);
            repository.DeleteTagSet(id);
        }
    }
}
