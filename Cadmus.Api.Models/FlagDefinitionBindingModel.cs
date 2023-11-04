using Cadmus.Core.Config;
using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Models;

public class FlagDefinitionBindingModel
{
    [Required]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Label { get; set; } = "";

    [MaxLength(100)]
    public string? Description { get; set; }

    [RegularExpression("^[0-9A-Fa-f]{6}$")]
    public string? ColorKey { get; set; }

    public FlagDefinition ToFlagDefinition()
    {
        return new FlagDefinition
        {
            Id = Id,
            Label = Label,
            Description = Description,
            ColorKey = ColorKey
        };
    }
}
