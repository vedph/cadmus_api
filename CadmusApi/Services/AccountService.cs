using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Model;
using CadmusApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CadmusApi.Services
{
    /// <summary>
    /// Account service.
    /// </summary>
    public sealed class AccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<MongoRole> _roleManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountService"/> class.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="roleManager">The role manager.</param>
        public AccountService(
            UserManager<ApplicationUser> userManager,
            RoleManager<MongoRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Registers the specified user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <param name="roles">The roles.</param>
        /// <returns></returns>
        public async Task<IActionResult> Register(ApplicationUser user, string password,
            IEnumerable<MongoRole> roles)
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

            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            // TODO: send email
            return new OkObjectResult($"User {user.Email} created, confirmation email sent.");
        }

        /// <summary>
        /// Confirms the specified email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public async Task<IActionResult> Confirm(string email, string code)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new BadRequestObjectResult($"Invalid email: {email}");

            IdentityResult result = await _userManager.ConfirmEmailAsync(user, code);

            if (!result.Succeeded)
                return new BadRequestObjectResult($"Bad confirmation code for email: {email}");

            return new OkObjectResult($"User {email} confirmed");
        }
    }
}
