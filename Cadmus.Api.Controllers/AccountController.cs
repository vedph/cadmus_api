﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using Cadmus.Api.Models;
using MessagingApi;
using Microsoft.Extensions.Logging;
using Cadmus.Api.Services.Auth;
using System.Linq;

namespace Cadmus.Api.Controllers;

/// <summary>
/// Account controller.
/// </summary>
/// <seealso cref="Controller" />
[ApiController]
public sealed class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMessageBuilderService _messageBuilder;
    private readonly IMailerService _mailer;
    private readonly MessagingOptions _options;
    private readonly ILogger<AccountController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountController" />
    /// class.
    /// </summary>
    /// <param name="userManager">The user manager.</param>
    /// <param name="messageBuilder">The message builder.</param>
    /// <param name="mailer">The mailer.</param>
    /// <param name="options">The options.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">null options or userManager or
    /// signInManager or environment or messageBuilder or mailer</exception>
    public AccountController(UserManager<ApplicationUser> userManager,
        IMessageBuilderService messageBuilder,
        IMailerService mailer,
        IOptions<MessagingOptions> options,
        ILogger<AccountController> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        _userManager = userManager ??
            throw new ArgumentNullException(nameof(userManager));
        _messageBuilder = messageBuilder ??
            throw new ArgumentNullException(nameof(messageBuilder));
        _mailer = mailer ?? throw new ArgumentNullException(nameof(mailer));
        _options = options.Value;
        _logger = logger ??
            throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Check if the specified email address is already registered.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <returns>model where entry=email address and isExisting=true if
    /// already registered</returns>
    [HttpGet]
    [Route("api/accounts/emailexists/{email}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> UserEmailExists(string email)
    {
        ApplicationUser? user = await _userManager.FindByEmailAsync(email);
        return Ok(new
        {
            Entry = email,
            IsExisting = user != null
        });
    }

    /// <summary>
    /// Check if the specified user name is already registered.
    /// </summary>
    /// <param name="name">The user name.</param>
    /// <returns>model where entry=name and isExisting=true if already
    /// registered</returns>
    [HttpGet]
    [Route("api/accounts/nameexists/{name}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> UserNameExists(string name)
    {
        ApplicationUser? user = await _userManager.FindByNameAsync(name);
        return Ok(new
        {
            Entry = name,
            IsExisting = user != null
        });
    }

    private IActionResult? GetErrorResult(IdentityResult result)
    {
        if (result == null) return StatusCode(500);

        if (!result.Succeeded)
        {
            if (result.Errors != null)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }

            // no ModelState errors are available to send, so just return
            // an empty BadRequest
            if (ModelState.IsValid) return BadRequest(ModelState);

            return BadRequest(ModelState);
        }

        return null;
    }

    private async Task SendConfirmEmailAsync(ApplicationUser user)
    {
        string token =
            await _userManager.GenerateEmailConfirmationTokenAsync(user);

        string url = _options.ApiRootUrl + Url.Action(
            "ConfirmRegistration", "Account",
            new { name = user.UserName, token });

        Message? message = _messageBuilder.BuildMessage("confirm-registration",
            new Dictionary<string, string>
            {
                ["UserName"] = user.UserName!,
                ["ConfirmationUrl"] = url
            });
        if (message != null)
            await _mailer.SendEmailAsync(user.Email!, user.UserName!, message);
    }

    /// <summary>
    /// Registers the specified user.
    /// </summary>
    /// <param name="model">The user model.</param>
    [Authorize(Roles = "admin")]
    [HttpPost]
    [Route("api/accounts/register")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterBindingModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _logger.LogInformation(
            "[ACCOUNT] {UserName} registering user {RegisteredUserName} " +
            "from {IP} ",
            User.Identity!.Name,
            model.Name,
            HttpContext.Connection.RemoteIpAddress);

        // ensure that email and user do not exist
        ApplicationUser? user = await _userManager.FindByEmailAsync(model.Email!);
        if (user != null)
        {
            _logger.LogWarning("Email address already registered: {RegisteredEmail}",
                model.Email);
            return BadRequest("Email address already registered");
        }

        user = await _userManager.FindByNameAsync(model.Name!);
        if (user != null)
        {
            _logger.LogWarning("User name already registered: {RegisteredUserName}",
                model.Name);
            return BadRequest("User name already registered");
        }

        // register
        user = new ApplicationUser
        {
            UserName = model.Name,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName
        };
        IdentityResult result =
            await _userManager.CreateAsync(user, model.Password!);
        if (!result.Succeeded)
        {
            _logger.LogError("Error registering user {RegisteredUserName}: {Error}",
                model.Name,
                string.Join("; ", from e in result.Errors select e.ToString()));
            return GetErrorResult(result) ?? Ok();
        }

        // log
        _logger.LogInformation(
            "[ACCOUNT] {UserName} registered user {RegisteredUserName} " +
            "from {IP} ",
            User.Identity.Name,
            user.UserName,
            HttpContext.Connection.RemoteIpAddress);

        // send email with confirmation token
        await SendConfirmEmailAsync(user);
        return Ok();
    }

    /// <summary>
    /// Resends the confirmation email.
    /// </summary>
    /// <param name="email">The email address to send a message to.</param>
    [Authorize]
    [HttpGet]
    [Route("api/accounts/resendconfirm/{email}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ResendConfirmRegistration(string email)
    {
        ApplicationUser? user = await _userManager.FindByEmailAsync(email);
        if (user == null) return BadRequest("Email address not registered");

        _logger.LogInformation("Resending email confirmation");
        await SendConfirmEmailAsync(user);
        _logger.LogInformation("Email confirmation re-sent");

        return Ok();
    }

    /// <summary>
    /// Confirms the registration.
    /// </summary>
    /// <param name="name">The user name.</param>
    /// <param name="token">The confirmation token.</param>
    [Authorize]
    [HttpGet]
    [Route("api/accounts/confirm")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ConfirmRegistration(
        [FromQuery] string name, [FromQuery] string token)
    {
        ApplicationUser? user = await _userManager.FindByNameAsync(name);
        if (user == null) return BadRequest();

        _logger.LogInformation("Confirming registration");

        // this API endpoint is accessed by users when clicking the
        // confirmation URL found in their email text. Thus, this endpoint
        // must reply with an HTML page for the end users.
        string? page;
        Dictionary<string, string> dct = new()
        {
            ["FirstName"] = user.FirstName!,
            ["LastName"] = user.LastName!,
            ["UserName"] = user.UserName!
        };

        // if already confirmed, do nothing
        if (user.EmailConfirmed)
        {
            _logger.LogInformation("Registration already confirmed");
            page = _messageBuilder.BuildMessage(
                "page-email-already-confirmed", dct)?.Content ?? "";
            return new FileStreamResult(
                new MemoryStream(Encoding.UTF8.GetBytes(page)), "text/html");
        }

        IdentityResult result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            _logger.LogInformation("Invalid confirmation token");
            page = _messageBuilder.BuildMessage(
                "page-invalid-email-confirm-token", dct)?.Content ?? "";
            return new FileStreamResult(
                new MemoryStream(Encoding.UTF8.GetBytes(page)), "text/html");
        }
        _logger.LogInformation("Registration successfully confirmed");

        _logger.LogInformation("Sending confirmation message");
        Message? message = _messageBuilder.BuildMessage(
            "registration-confirmed", new Dictionary<string, string>
            {
                ["FirstName"] = user.FirstName!,
                ["LastName"] = user.LastName!,
                ["UserName"] = user.UserName!
            });
        if (message != null)
        {
            await _mailer.SendEmailAsync(user.Email!, user.UserName!, message);
            _logger.LogInformation("Confirmation message sent");
        }

        page = _messageBuilder.BuildMessage("page-email-confirmed", dct)
            ?.Content ?? "";
        return new FileStreamResult(new MemoryStream(
            Encoding.UTF8.GetBytes(page)), "text/html");
    }

    /// <summary>
    /// Changes the user's password.
    /// </summary>
    /// <param name="model">The change password model.</param>
    [Authorize]
    [HttpPost]
    [Route("api/accounts/changepassword")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordBindingModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _logger.LogInformation("Change password request for {UserEmail}",
            model.Email);

        ApplicationUser? user = await _userManager.FindByEmailAsync(model.Email!);
        if (user == null)
        {
            _logger.LogWarning("Change password: no user with email {UserEmail}",
                model.Email);
            return BadRequest();
        }

        IdentityResult result = await _userManager.ChangePasswordAsync(user,
            model.OldPassword!,
            model.NewPassword!);

        if (!result.Succeeded)
        {
            _logger.LogError("Error changing password for {UserEmail}: {Error}",
                model.Email,
                string.Join("; ", from e in result.Errors select e.ToString()));

            return GetErrorResult(result) ?? Ok();
        }
        _logger.LogInformation("Successfully changed password for {UserEmail}",
            model.Email);

        return Ok();
    }

    /// <summary>
    /// Requests the password reset. This generates an email message to the
    /// requester, with a special link to follow to effectively reset his
    /// password.
    /// </summary>
    /// <param name="model">The reset request model.</param>
    [HttpPost]
    [Route("api/accounts/resetpassword/request")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> RequestPasswordReset(
        ResetPasswordRequestBindingModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _logger.LogInformation("Reset password request for {UserEmail}",
            model.Email);

        ApplicationUser? user = await _userManager.FindByEmailAsync(model.Email!);
        if (user == null)
        {
            _logger.LogWarning("Reset password: no user with email {UserEmail}",
                model.Email);
            return BadRequest();
        }

        string token = await _userManager.GeneratePasswordResetTokenAsync(user);
        _logger.LogInformation("Reset password token generated for {UserEmail}",
            model.Email);

        string url = _options.ApiRootUrl + Url.Action(
            "ResetPassword", "Account",
            new { name = user.UserName, token });

        _logger.LogInformation("Sending reset password token to {UserEmail}",
            model.Email);
        Message? message = _messageBuilder.BuildMessage("reset-password",
            new Dictionary<string, string>
            {
                ["UserName"] = user.UserName!,
                ["ResetUrl"] = url
            });
        if (message != null)
        {
            await _mailer.SendEmailAsync(user.Email!, user.UserName!, message);
            _logger.LogInformation("Reset password token sent to {UserEmail}",
                model.Email);
        }

        return Ok();
    }

    /// <summary>
    /// Resets the password using the received token.
    /// </summary>
    /// <param name="name">The user name.</param>
    /// <param name="token">The password reset token.</param>
    [HttpGet]
    [Route("api/accounts/resetpassword/apply")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ResetPassword([FromQuery] string name,
        [FromQuery] string token)
    {
        _logger.LogInformation("Apply password reset for {UserName}", name);

        ApplicationUser? user = await _userManager.FindByNameAsync(name);
        if (user == null)
        {
            _logger.LogWarning("Apply password reset: user {UserName} not found",
                name);
            return BadRequest();
        }

        _logger.LogInformation("Setting new password for {UserName}", name);
        PasswordGenerator generator = new();
        string newPassword = generator.Generate();

        IdentityResult result =
            await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            _logger.LogError("Error setting new password for {UserName}: {Error}",
                name,
               string.Join("; ", from e in result.Errors select e.ToString()));
            return GetErrorResult(result) ?? Ok();
        }

        _logger.LogInformation("Sending new password to {UserEmail}", user.Email);
        Message? message = _messageBuilder.BuildMessage("password-reset",
            new Dictionary<string, string>
            {
                ["FirstName"] = user.FirstName!,
                ["LastName"] = user.LastName!,
                ["UserName"] = user.UserName!,
                ["NewPassword"] = newPassword
            });
        if (message != null)
        {
            await _mailer.SendEmailAsync(user.Email!, user.UserName!, message);
            _logger.LogInformation("New password sent to {UserEmail}", user.Email);
        }

        return Ok();
    }

    /// <summary>
    /// Delete the user with the specified username.
    /// </summary>
    /// <param name="name">The user name</param>
    [Authorize(Roles = "admin")]
    [HttpDelete("api/accounts/{name}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> DeleteUser(string name)
    {
        _logger.LogInformation("{UserName} deleting user {DeletedUserName}",
            User.Identity!.Name,
            name);

        if (User.Identity.Name == name)
        {
            _logger.LogWarning(
                "Delete user: user {DeletedUserName} cannot delete himself",
                name);
            return BadRequest($"User {name} cannot delete himself");
        }

        ApplicationUser? user = await _userManager.FindByNameAsync(name);
        if (user == null)
        {
            _logger.LogWarning("Delete user: user {DeletedUserName} not found",
                name);
            return BadRequest("No such user: " + name);
        }

        await _userManager.DeleteAsync(user);

        _logger.LogInformation(
            "[ACCOUNT] {UserName} deleted user {DeletedUserName} " +
            "from {IP}",
            User.Identity.Name,
            name,
            HttpContext.Connection.RemoteIpAddress);

        return NoContent();
    }
}