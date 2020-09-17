/*
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using MessagingApi;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Cadmus.Api.Services.Auth
{
    /// <summary>
    /// Account service.
    /// </summary>
    public sealed class AccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMessageBuilderService _messageBuilder;
        private readonly IMailerService _mailer;
        private readonly MessagingOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountService"/> class.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="roleManager">The role manager.</param>
        /// <param name="messageBuilder">The message builder.</param>
        /// <param name="mailer">The mailer.</param>
        /// <param name="options">The options.</param>
        public AccountService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IMessageBuilderService messageBuilder,
            IMailerService mailer,
            IOptions<MessagingOptions> options)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _messageBuilder = messageBuilder;
            _mailer = mailer;
            _options = options.Value;
        }

        /// <summary>
        /// Registers the specified user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <param name="roles">The roles.</param>
        /// <returns>Result.</returns>
        public async Task<IActionResult> Register(ApplicationUser user,
            string password,
            IEnumerable<ApplicationRole> roles)
        {
            if (await _userManager.FindByEmailAsync(user.Email) != null)
                return new BadRequestObjectResult($"User {user.Email} already exists.");

            IdentityResult result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return new ObjectResult($"User {user.Email} already exists.")
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }

            if (roles != null)
            {
                foreach (var role in roles)
                    await _userManager.AddToRoleAsync(user, role.Name);
            }

            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            string url = _options.ApiRootUrl + Url.Action(
                "ConfirmRegistration", "Account",
                new { name = user.UserName, token });

            Message message = _messageBuilder.BuildMessage("confirm-registration",
                new Dictionary<string, string>
                {
                    ["UserName"] = user.UserName,
                    ["ConfirmationUrl"] = url
                });
            await _mailer.SendEmailAsync(user.Email, user.UserName, message);

            return new OkObjectResult(
                $"User {user.Email} created, confirmation email sent.");
        }

        /// <summary>
        /// Confirms the specified email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="code">The code.</param>
        /// <returns>Result.</returns>
        public async Task<IActionResult> Confirm(string email, string code)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new BadRequestObjectResult($"Invalid email: {email}");

            IdentityResult result = await _userManager.ConfirmEmailAsync(user, code);

            if (!result.Succeeded)
            {
                return new BadRequestObjectResult(
                    $"Bad confirmation code for email: {email}");
            }

            return new OkObjectResult($"User {email} confirmed");
        }
    }
}
*/