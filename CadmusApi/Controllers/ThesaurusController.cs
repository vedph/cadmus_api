using System;
using System.Collections.Generic;
using System.Linq;
using Cadmus.Core;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using CadmusApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CadmusApi.Controllers
{
    /// <summary>
    /// Thesauri controller.
    /// </summary>
    [Authorize]
    [ApiController]
    public sealed class ThesaurusController : Controller
    {
        private readonly IRepositoryProvider _repositoryProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThesaurusController"/>
        /// class.
        /// </summary>
        /// <param name="repositoryProvider">The repository provider.</param>
        /// <exception cref="ArgumentNullException">repository</exception>
        public ThesaurusController(IRepositoryProvider repositoryProvider)
        {
            _repositoryProvider = repositoryProvider ??
                                 throw new ArgumentNullException(nameof(repositoryProvider));
        }

        /// <summary>
        /// Gets the list of all the thesauri IDs.
        /// </summary>
        /// <param name="database">The name of the Mongo database.</param>
        /// <returns>list of tag sets IDs</returns>
        [HttpGet("api/{database}/thesauri")]
        public IActionResult GetSetIds([FromRoute] string database)
        {
            ICadmusRepository repository =
                _repositoryProvider.CreateRepository(database);
            return Ok(repository.GetThesaurusIds());
        }

        /// <summary>
        /// Gets the thesaurus with the specified ID. If the ID does not include
        /// the language suffix (<c>@</c> + ISO639-2 letters code, e.g. <c>@en</c>),
        /// or the requested language does not exist, the method attempts to
        /// fallback to the default language (<c>eng</c> or <c>en</c>=English).
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="id">The thesaurus ID.</param>
        /// <param name="emptyIfNotFound">True to return an empty thesaurus
        /// rather than a 404 when the ID was not found.</param>
        /// <returns>Thesaurus</returns>
        [HttpGet("api/{database}/thesauri/{id}", Name = "GetThesaurus")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public IActionResult GetThesaurus(
            [FromRoute] string database,
            [FromRoute] string id,
            [FromQuery] bool emptyIfNotFound)
        {
            ICadmusRepository repository =
                _repositoryProvider.CreateRepository(database);
            Thesaurus thesaurus = repository.GetThesaurus(id);
            if (thesaurus == null)
            {
                return emptyIfNotFound
                    ? Ok(new ThesaurusModel
                    {
                        Id = id,
                        Language = "en",
                        Entries = Array.Empty<ThesaurusEntry>()
                    })
                    : (IActionResult)NotFound();
            }

            ThesaurusModel model = new ThesaurusModel
            {
                Id = thesaurus.Id,
                Language = thesaurus.GetLanguage(),
                Entries = thesaurus.GetEntries().ToArray()
            };

            return Ok(model);
        }

        /// <summary>
        /// Gets the specified set of thesauri. If any of the IDs does not include
        /// the language suffix (<c>@</c> + ISO639-2 letters code, e.g. <c>@en</c>),
        /// or the requested language does not exist, the method attempts to
        /// fallback to the default language (<c>eng</c> or <c>en</c>=English).
        /// Only the thesauri which were found are returned.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="ids">The thesauri IDs, separated by commas.</param>
        /// <returns>Object where each key is a thesaurus ID with a value
        /// equal to the thesaurus model.</returns>
        [HttpGet("api/{database}/thesauri-set/{ids}")]
        [ProducesResponseType(200)]
        public ActionResult<Dictionary<string,ThesaurusModel>> GetThesauri(
            [FromRoute] string database, [FromRoute] string ids)
        {
            ICadmusRepository repository =
                _repositoryProvider.CreateRepository(database);
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
        /// Adds or updates the specified thesaurus.
        /// </summary>
        /// <param name="database">The database ID.</param>
        /// <param name="model">The thesaurus model.</param>
        [HttpPost("api/{database}/thesauri")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public IActionResult AddThesaurus(
            [FromRoute] string database,
            [FromBody] ThesaurusBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            ICadmusRepository repository = _repositoryProvider.CreateRepository(database);
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
        /// Deletes the thesaurus with the specified ID.
        /// </summary>
        /// <param name="database">The database ID.</param>
        /// <param name="id">The thesaurus ID.</param>
        [HttpDelete("api/{database}/thesauri/{id}")]
        public void DeleteThesaurus(
            [FromRoute] string database,
            [FromRoute] string id)
        {
            ICadmusRepository repository =
                _repositoryProvider.CreateRepository(database);
            repository.DeleteThesaurus(id);
        }
    }
}
