using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Models;

/// <summary>
/// Registration binding model.
/// </summary>
public class RegisterBindingModel
{
    /// <summary>
    /// The email address.
    /// </summary>
    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>
    /// The password.
    /// </summary>
    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    /// <summary>
    /// The user name.
    /// </summary>
    [Required]
    [MaxLength(256, ErrorMessage = "User name too long")]
    public string? Name { get; set; }

    /// <summary>
    /// The user's real first name.
    /// </summary>
    [Required]
    [MaxLength(50, ErrorMessage = "First name too long")]
    public string? FirstName { get; set; }

    /// <summary>
    /// The user's real last name.
    /// </summary>
    [Required]
    [MaxLength(50, ErrorMessage = "Last name too long")]
    public string? LastName { get; set; }
}
