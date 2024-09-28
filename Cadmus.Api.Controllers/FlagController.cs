using System;
using System.Linq;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using Cadmus.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cadmus.Api.Models;

namespace Cadmus.Api.Controllers;

/// <summary>
/// Items flags controller.
/// </summary>
[Authorize]
[ApiController]
public sealed class FlagController : ControllerBase
{
    private readonly IRepositoryProvider _repositoryProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlagController"/> class.
    /// </summary>
    /// <param name="repositoryProvider">The repository provider.</param>
    /// <exception cref="ArgumentNullException">repository</exception>
    public FlagController(IRepositoryProvider repositoryProvider)
    {
        _repositoryProvider = repositoryProvider ??
            throw new ArgumentNullException(nameof(repositoryProvider));
    }

    /// <summary>
    /// Gets the list of all the items flags definitions.
    /// </summary>
    /// <returns>list of flags definitions</returns>
    [HttpGet("api/flags")]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    public ActionResult<FlagDefinition[]> Get()
    {
        ICadmusRepository repository = _repositoryProvider.CreateRepository();
        return Ok(repository.GetFlagDefinitions().ToArray());
    }

    /// <summary>
    /// Adds the flags.
    /// </summary>
    /// <param name="flags">The flags.</param>
    [HttpPost("api/flags")]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    public void AddFlags([FromBody] FlagDefinitionBindingModel[] flags)
    {
        ICadmusRepository repository = _repositoryProvider.CreateRepository();

        foreach (FlagDefinitionBindingModel flag in flags)
            repository.AddFlagDefinition(flag.ToFlagDefinition());
    }
}
