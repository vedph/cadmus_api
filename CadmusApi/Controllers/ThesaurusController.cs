using System;
using System.Collections.Generic;
using System.Linq;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using CadmusApi.Models;
using CadmusApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CadmusApi.Controllers
{
    /// <summary>
    /// Thesauri controller.
    /// </summary>
    // [Authorize]
    [ApiController]
    public sealed class ThesaurusController : Controller
    {
        private readonly RepositoryService _repositoryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThesaurusController"/> class.
        /// </summary>
        /// <param name="repositoryService">The repository service.</param>
        /// <exception cref="ArgumentNullException">repository</exception>
        public ThesaurusController(RepositoryService repositoryService)
        {
            _repositoryService = repositoryService ??
                                 throw new ArgumentNullException(nameof(repositoryService));
        }

        /// <summary>
        /// Gets the list of all the tag sets IDs.
        /// </summary>
        /// <param name="database">The name of the Mongo database.</param>
        /// <returns>list of tag sets IDs</returns>
        [HttpGet("api/{database}/thesauri")]
        public IActionResult GetSetIds(string database)
        {
            ICadmusRepository repository = _repositoryService.CreateRepository(database);
            return Ok(repository.GetThesaurusIds());
        }

        /// <summary>
        /// Gets the tags set with the specified ID.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="id">The tags set ID.</param>
        /// <returns>set</returns>
        [HttpGet("api/{database}/thesauri/{id}", Name = "GetThesaurus")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ActionResult<ThesaurusModel> GetThesaurus(string database, string id)
        {
            ICadmusRepository repository =
                _repositoryService.CreateRepository(database);
            Thesaurus thesaurus = repository.GetThesaurus(id);
            if (thesaurus == null) return NotFound();

            ThesaurusModel model = new ThesaurusModel
            {
                Id = thesaurus.Id,
                Language = thesaurus.GetLanguage(),
                Entries = thesaurus.GetEntries().ToArray()
            };

            return Ok(model);
        }

        /// <summary>
        /// Gets the specified set of thesauri.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="ids">The thesauri IDs, separated by commas.</param>
        /// <returns>Object where each key is a thesaurus ID with a value
        /// equal to the thesaurus model.</returns>
        [HttpGet("api/{database}/thesauri-set/{ids}")]
        [ProducesResponseType(200)]
        public ActionResult<Dictionary<string,ThesaurusModel>> GetThesauri(
            string database, string ids)
        {
            ICadmusRepository repository =
                _repositoryService.CreateRepository(database);
            Dictionary<string, ThesaurusModel> dct =
                new Dictionary<string, ThesaurusModel>();

            foreach (string id in (ids ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Distinct())
            {
                Thesaurus thesaurus = repository.GetThesaurus(id);
                if (thesaurus == null) continue;
                dct[id] = new ThesaurusModel
                {
                    Id = thesaurus.Id,
                    Language = thesaurus.GetLanguage(),
                    Entries = thesaurus.GetEntries().ToArray()
                };
            }
            return Ok(dct);
        }

        /// <summary>
        /// Adds or updates the specified tags set.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="model">The tags set model.</param>
        [HttpPost("api/{database}/thesauri")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public IActionResult AddThesaurus(string database,
            [FromBody] ThesaurusBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            ICadmusRepository repository = _repositoryService.CreateRepository(database);
            Thesaurus thesaurus = new Thesaurus(model.Id);
            foreach (ThesaurusEntryBindingModel entry in model.Entries)
                thesaurus.AddEntry(new ThesaurusEntry(entry.Id, entry.Value));
            repository.AddThesaurus(thesaurus);

            return CreatedAtRoute("GetThesaurus", new
            {
                database,
                id = thesaurus.Id
            }, thesaurus);
        }

        /// <summary>
        /// Deletes the tags set with the specified ID.
        /// </summary>
        /// <param name="database">The database ID.</param>
        /// <param name="id">The tags set ID.</param>
        [HttpDelete("api/{database}/thesauri/{id}")]
        public void DeleteSet(string database, string id)
        {
            ICadmusRepository repository =
                _repositoryService.CreateRepository(database);
            repository.DeleteThesaurus(id);
        }
    }
}
