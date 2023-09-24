using System.Collections.Generic;

namespace Cadmus.Api.Models;

/// <summary>
/// Result of a thesauri import.
/// </summary>
public class ImportThesauriResult
{
    /// <summary>
    /// Gets or sets the IDs of the imported thesauri.
    /// </summary>
    public IList<string> ImportedIds { get; set; }

    /// <summary>
    /// Gets or sets the error message if any.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportThesauriResult"/> class.
    /// </summary>
    public ImportThesauriResult()
    {
        ImportedIds = new List<string>();
    }
}
