using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Models;

/// <summary>
/// Login model.
/// </summary>
public sealed class LoginBindingModel
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    [Required(ErrorMessage = "User Name is required")]
    [MaxLength(100)]
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [MaxLength(100)]
    public string? Password { get; set; }
}
