using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Models;

/// <summary>
/// Import thesauri binding model.
/// </summary>
public class ImportThesauriBindingModel
{
    /// <summary>
    /// The import mode. This is a letter: <c>R</c>=replace: if the imported
    /// thesaurus already exists, it is fully replaced by the new one;
    /// <c>P</c>=patch: the existing thesaurus is patched with the imported one:
    /// any existing entry has its value overwritten; any non existing entry
    /// is just added; <c>S</c>=synch: equal to patch, with the addition that
    /// any existing entry not found in the imported thesaurus is removed.
    /// </summary>
    [RegularExpression("^[rpsRPS]$")]
    public string? Mode { get; set; }

    /// <summary>
    /// The sheet number (1-N), used when importing from Excel.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int? ExcelSheet { get; set; }

    /// <summary>
    /// The start row number, used when importing from Excel.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int? ExcelRow { get; set; }

    /// <summary>
    /// The start column number, used when importing from Excel.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int? ExcelColumn { get; set; }

    /// <summary>
    /// The dry run flag, which, if true, causes the import to be performed
    /// without actually saving anything.
    /// </summary>
    public bool? DryRun { get; set; }
}
