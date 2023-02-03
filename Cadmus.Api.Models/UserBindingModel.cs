using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Models;

/// <summary>
/// User's editable data binding model.
/// </summary>
public sealed class UserBindingModel
{
    /// <summary>
    /// The user's nickname.
    /// </summary>
    [Required]
    public string? UserName { get; set; }

    /// <summary>
    /// The user's email address.
    /// </summary>
    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>
    /// True if the user's email address was confirmed.
    /// </summary>
    [Required]
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// True if the user lockout is enabled.
    /// </summary>
    [Required]
    public bool LockoutEnabled { get; set; }

    /// <summary>
    /// Gets or sets the lockout end date and time.
    /// </summary>
    public DateTime? LockoutEnd { get; set; }

    /// <summary>
    /// The user's first name.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string? FirstName { get; set; }

    /// <summary>
    /// The user's last name.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string? LastName { get; set; }

    /// <summary>
    /// The user's roles.
    /// </summary>
    public IList<string>? Roles { get; set; }
}
