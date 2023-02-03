using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Models;

/// <summary>
/// Change password binding model.
/// </summary>
public sealed class ChangePasswordBindingModel
{
    /// <summary>
    /// The email address.
    /// </summary>
    [Required]
    public string? Email { get; set; }

    /// <summary>
    /// The old password.
    /// </summary>
    [Required]
    [DataType(DataType.Password)]
    public string? OldPassword { get; set; }

    /// <summary>
    /// The new password.
    /// </summary>
    [Required]
    [StringLength(100, ErrorMessage = "{0} must be at least {2} characters long", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }

    /// <summary>
    /// The confirmation password.
    /// </summary>
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "The new password is different from the confirmation password")]
    public string? ConfirmPassword { get; set; }
}
