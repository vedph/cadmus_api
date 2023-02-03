using System;
using System.Collections.Generic;
using System.Linq;
using Cadmus.Core;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using Cadmus.Api.Models;
using Fusi.Tools.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CadmusApi.Controllers;

/// <summary>
/// Thesauri controller.
/// </summary>
[Authorize]
[ApiController]
public sealed class ThesaurusController : Controller
{
    private readonly IRepositoryProvider _repositoryProvider;
    private readonly ILogger<ThesaurusController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThesaurusController"/>
    /// class.
    /// </summary>
    /// <param name="repositoryProvider">The repository provider.</param>
    /// <exception cref="ArgumentNullException">repository</exception>
    public ThesaurusController(IRepositoryProvider repositoryProvider,
        ILogger<ThesaurusController> logger)
    {
        _repositoryProvider = repositoryProvider ??
                             throw new ArgumentNullException(nameof(repositoryProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the list of all the thesauri IDs.
    /// </summary>
    /// <returns>list of tag sets IDs</returns>
    [HttpGet("api/thesauri-ids")]
    public IActionResult GetSetIds(
        [FromQuery] ThesaurusLookupBindingModel filter)
    {
        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();

        return Ok(filter.IsEmpty()
            ? repository.GetThesaurusIds()
            : repository.GetThesaurusIds(new ThesaurusFilter
            {
                PageNumber = 1,
                PageSize = filter.Limit,
                Id = filter.Id,
                IsAlias = filter.IsAlias,
                Language = filter.Language,
            }));
    }

    /// <summary>
    /// Gets the thesaurus with the specified ID. If the ID does not include
    /// the language suffix (<c>@</c> + ISO639-2 letters code, e.g. <c>@en</c>),
    /// or the requested language does not exist, the method attempts to
    /// fallback to the default language (<c>eng</c> or <c>en</c>=English).
    /// </summary>
    /// <param name="id">The thesaurus ID.</param>
    /// <param name="emptyIfNotFound">True to return an empty thesaurus
    /// rather than a 404 when the ID was not found.</param>
    /// <returns>Thesaurus</returns>
    [HttpGet("api/thesauri/{id}", Name = "GetThesaurus")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public IActionResult GetThesaurus(
        [FromRoute] string id,
        [FromQuery] bool emptyIfNotFound)
    {
        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();
        Thesaurus? thesaurus = repository.GetThesaurus(id);
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

        ThesaurusModel model = new(thesaurus);

        return Ok(model);
    }

    /// <summary>
    /// Gets the specified page of thesauri matching the filter.
    /// </summary>
    /// <param name="model">The filter model.</param>
    /// <returns>Page.</returns>
    [HttpGet("api/thesauri")]
    [ProducesResponseType(200)]
    public IActionResult GetThesauri(
        [FromQuery] ThesaurusFilterBindingModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();

        DataPage<Thesaurus> page = repository.GetThesauri(new ThesaurusFilter
        {
            PageNumber = model.PageNumber,
            PageSize = model.PageSize,
            Id = model.Id,
            IsAlias = model.IsAlias,
            Language = model.Language
        });

        DataPage<ThesaurusModel> result = new(
            page.PageNumber,
            page.PageSize,
            page.Total,
            (from t in page.Items
             select new ThesaurusModel(t)).ToList());
        return Ok(result);
    }

    private static string PurgeThesaurusId(string id)
    {
        int i = id.LastIndexOf('.');
        if (i > -1) return id[..i];
        i = id.LastIndexOf('@');
        return i > -1 ? id[..i] : id;
    }

    /// <summary>
    /// Gets the specified set of thesauri. If any of the IDs does not include
    /// the language suffix (<c>@</c> + ISO639-2 letters code, e.g. <c>@en</c>),
    /// or the requested language does not exist, the method attempts to
    /// fallback to the default language (<c>eng</c> or <c>en</c>=English).
    /// Only the thesauri which were found are returned.
    /// </summary>
    /// <param name="ids">The thesauri IDs, separated by commas.</param>
    /// <param name="purgeIds">True to purge the keys (=the thesauri IDs)
    /// of the returned dictionary, so that any scope and language suffixes
    /// get removed. Scope starts from the last dot, language from the last
    /// <c>@</c> character. For instance, if the requested ID is
    /// <c>apparatus-witnesses.verg-eclo@en</c> and purging is enabled,
    /// the dictionary key to this thesaurus will be <c>apparatus-witnesses</c>.
    /// </param>
    /// <returns>Object where each key is a thesaurus ID with a value
    /// equal to the thesaurus model.</returns>
    [HttpGet("api/thesauri-set")]
    [ProducesResponseType(200)]
    public ActionResult<Dictionary<string,ThesaurusModel>> GetThesauriSet(
        [FromQuery] string ids,
        [FromQuery] bool purgeIds)
    {
        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();
        Dictionary<string, ThesaurusModel> dct =
            new();

        foreach (string id in (ids ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Distinct())
        {
            Thesaurus? thesaurus = repository.GetThesaurus(id);
            if (thesaurus == null) continue;
            dct[purgeIds ? PurgeThesaurusId(id) : id] = new ThesaurusModel
            {
                Id = thesaurus.Id,
                Language = thesaurus.GetLanguage(),
                Entries = thesaurus.Entries?.ToArray()
                    ?? Array.Empty<ThesaurusEntry>()
            };
        }
        return Ok(dct);
    }

    /// <summary>
    /// Adds or updates the specified thesaurus.
    /// </summary>
    /// <param name="model">The thesaurus model.</param>
    [Authorize(Roles = "admin,editor,operator")]
    [HttpPost("api/thesauri")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public IActionResult AddThesaurus(
        [FromBody] ThesaurusBindingModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _logger.LogInformation("User {UserName} adding thesaurus {ThesaurusId}",
            User.Identity!.Name,
            model.Id);

        ICadmusRepository repository = _repositoryProvider.CreateRepository();
        Thesaurus thesaurus = new(model.Id!);
        foreach (ThesaurusEntryBindingModel entry in model.Entries!)
            thesaurus.AddEntry(new ThesaurusEntry(entry.Id!, entry.Value!));
        repository.AddThesaurus(thesaurus);

        _logger.LogInformation(
            "User {UserName} successfully added thesaurus {ThesaurusId}",
            User.Identity.Name,
            model.Id);

        return CreatedAtRoute("GetThesaurus", new
        {
            id = thesaurus.Id
        }, thesaurus);
    }

    /// <summary>
    /// Deletes the thesaurus with the specified ID.
    /// </summary>
    /// <param name="id">The thesaurus ID.</param>
    [Authorize(Roles = "admin,editor")]
    [HttpDelete("api/thesauri/{id}")]
    public void DeleteThesaurus(
        [FromRoute] string id)
    {
        _logger.LogInformation("User {UserName} deleting thesaurus {ThesaurusId}",
            User.Identity!.Name,
            id);

        ICadmusRepository repository =
            _repositoryProvider.CreateRepository();
        repository.DeleteThesaurus(id);

        _logger.LogInformation(
            "User {UserName} successfully deleted thesaurus {ThesaurusId}",
            User.Identity.Name,
            id);
    }
}
