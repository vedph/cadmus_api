using Cadmus.Api.Models;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using Cadmus.Import.Excel;
using Cadmus.Import;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System;
using Microsoft.Extensions.Logging;
using Cadmus.Core;

namespace Cadmus.Api.Controllers.Import;

/// <summary>
/// Thesauri import controller.
/// </summary>
/// <seealso cref="ControllerBase" />
[Authorize]
[ApiController]
[Tags("Thesaurus")]
public sealed class ThesaurusImportController : ControllerBase
{
    private readonly IRepositoryProvider _repositoryProvider;
    private readonly ILogger<ThesaurusImportController> _logger;

    public ThesaurusImportController(IRepositoryProvider repositoryProvider,
        ILogger<ThesaurusImportController> logger)
    {
        _repositoryProvider = repositoryProvider;
        _logger = logger;
    }

    private static ImportUpdateMode GetMode(char c)
    {
        return char.ToUpperInvariant(c) switch
        {
            'P' => ImportUpdateMode.Patch,
            'S' => ImportUpdateMode.Synch,
            _ => ImportUpdateMode.Replace,
        };
    }

    /// <summary>
    /// Uploads one or more thesauri importing them into the Cadmus database.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="model">The import model.</param>
    /// <returns>Result.</returns>
    /// <exception cref="InvalidOperationException">No ID for thesaurus</exception>
    [Authorize(Roles = "admin")]
    [HttpPost("api/thesauri/import")]
    public ImportThesauriResult UploadThesauri(
        // https://github.com/domaindrivendev/Swashbuckle.AspNetCore#handle-forms-and-file-uploads
        IFormFile file,
        [FromQuery] ImportThesauriBindingModel model)
    {
        _logger?.LogInformation("User {UserName} importing thesauri from " +
            "{FileName} from {IP} (dry={IsDry})",
            User.Identity!.Name,
            file.FileName,
            HttpContext.Connection.RemoteIpAddress,
            model.DryRun == true);

        ICadmusRepository repository = _repositoryProvider.CreateRepository();

        ExcelThesaurusReaderOptions xlsOptions = new()
        {
            SheetIndex = model.ExcelSheet == null ? 0 : model.ExcelSheet.Value - 1,
            RowOffset = model.ExcelRow == null ? 0 : model.ExcelRow.Value - 1,
            ColumnOffset = model.ExcelColumn == null ? 0 : model.ExcelColumn.Value - 1
        };

        try
        {
            Stream stream = file.OpenReadStream();
            using IThesaurusReader reader =
                Path.GetExtension(file.FileName).ToLowerInvariant() switch
                {
                    ".csv" => new CsvThesaurusReader(stream),
                    ".xls" => new ExcelThesaurusReader(stream, xlsOptions),
                    ".xlsx" => new ExcelThesaurusReader(stream),
                    _ => new JsonThesaurusReader(stream)
                };

            Thesaurus? source;
            ImportUpdateMode mode = GetMode(model.Mode?[0] ?? 'R');
            List<string> ids = new();

            while ((source = reader.Next()) != null)
            {
                if (string.IsNullOrEmpty(source.Id))
                    throw new InvalidOperationException("No ID for thesaurus");

                _logger?.LogInformation("Importing thesaurus ID: {Id}", source.Id);
                ids.Add(source.Id);

                // fetch from repository
                Thesaurus? target = repository.GetThesaurus(source.Id);

                // import
                Thesaurus result = ThesaurusHelper.CopyThesaurus(
                    source, target, mode);

                // save
                if (model.DryRun != true) repository.AddThesaurus(result);
            }

            return new ImportThesauriResult
            {
                ImportedIds = ids
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error importing thesauri: {Message}",
                ex.Message);
            return new ImportThesauriResult
            {
                Error = ex.Message
            };
        }
    }
}
