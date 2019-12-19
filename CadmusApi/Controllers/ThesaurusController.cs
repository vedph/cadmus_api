using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using CadmusApi.Models;
using CadmusApi.Services;
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
        private readonly RepositoryService _repositoryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThesaurusController"/>
        /// class.
        /// </summary>
        /// <param name="repositoryService">The repository service.</param>
        /// <exception cref="ArgumentNullException">repository</exception>
        public ThesaurusController(RepositoryService repositoryService)
        {
            _repositoryService = repositoryService ??
                                 throw new ArgumentNullException(nameof(repositoryService));
        }

        /// <summary>
        /// Gets the list of all the thesauri IDs.
        /// </summary>
        /// <param name="database">The name of the Mongo database.</param>
        /// <returns>list of tag sets IDs</returns>
        [HttpGet("api/{database}/thesauri")]
        public IActionResult GetSetIds(string database)
        {
            ICadmusRepository repository =
                _repositoryService.CreateRepository(database);
            return Ok(repository.GetThesaurusIds());
        }

        private static string GetFallbackId(string id, string defaultLang = "en")
        {
            Regex r = new Regex(@"^(?<id>.+)\@(?<lang>[a-z]{2})$");
            Match m = r.Match(id);

            // bare ID or non-default language: append @language
            if (!m.Success || defaultLang != m.Groups["lang"].Value)
                return id + "@" + defaultLang;

            // else no fallback possible
            return null;
        }

        private static Thesaurus GetThesaurusWithFallback(string id,
            ICadmusRepository repository)
        {
            Thesaurus thesaurus = repository.GetThesaurus(id);
            if (thesaurus != null) return thesaurus;

            string fallbackId = GetFallbackId(id);
            if (fallbackId != null)
                thesaurus = repository.GetThesaurus(fallbackId);
            return thesaurus;
        }

        /// <summary>
        /// Gets the thesaurus with the specified ID. If the ID does not include
        /// the language suffix (<c>@</c> + ISO639-2 letters code, e.g. <c>@en</c>),
        /// or the requested language does not exist, the method attempts to
        /// fallback to the default language (<c>en</c>=English).
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="id">The thesaurus ID.</param>
        /// <returns>Thesaurus</returns>
        [HttpGet("api/{database}/thesauri/{id}", Name = "GetThesaurus")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ActionResult<ThesaurusModel> GetThesaurus(string database, string id)
        {
            ICadmusRepository repository =
                _repositoryService.CreateRepository(database);
            Thesaurus thesaurus = GetThesaurusWithFallback(id, repository);
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
        /// Gets the specified set of thesauri. If any of the IDs does not include
        /// the language suffix (<c>@</c> + ISO639-2 letters code, e.g. <c>@en</c>),
        /// or the requested language does not exist, the method attempts to
        /// fallback to the default language (<c>en</c>=English). Only the
        /// thesauri which were found are returned.
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
                Thesaurus thesaurus = GetThesaurusWithFallback(id, repository);
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
        /// Deletes the thesaurus with the specified ID.
        /// </summary>
        /// <param name="database">The database ID.</param>
        /// <param name="id">The thesaurus ID.</param>
        [HttpDelete("api/{database}/thesauri/{id}")]
        public void DeleteThesaurus(string database, string id)
        {
            ICadmusRepository repository =
                _repositoryService.CreateRepository(database);
            repository.DeleteThesaurus(id);
        }
    }
}
