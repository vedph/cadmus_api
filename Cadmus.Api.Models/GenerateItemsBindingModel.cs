using System;
using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Models;

/// <summary>
/// Model for generating items from a template item.
/// </summary>
public class GenerateItemsBindingModel
{
    [Range(1, 100)]
    public int Count { get; set; }

    [Required]
    public string? ItemId { get; set; }

    [Required]
    public string? Title { get; set; }

    [Range(0, 0xFFFF)]
    public int Flags { get; set; }
}
