using System;
using System.Collections.Generic;

namespace Cadmus.Api.Models;

/// <summary>
/// User.
/// </summary>
public sealed class UserModel
{
    /// <summary>
    /// Gets or sets the name of the user.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Gets or sets the user's email.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the roles.
    /// </summary>
    public IList<string>? Roles { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the email of this
    /// user has been confirmed.
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// True if the user lockout is enabled.
    /// </summary>
    public bool LockoutEnabled { get; set; }

    /// <summary>
    /// Gets or sets the lockout end date and time.
    /// </summary>
    public DateTime? LockoutEnd { get; set; }
}
