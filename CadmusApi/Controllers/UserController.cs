using System;
using System.Collections.Generic;
using System.Linq;
using AspNet.Security.OAuth.Validation;
using CadmusApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CadmusApi.Controllers
{
    /// <summary>
    /// User controller
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    // [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
    public sealed class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <exception cref="ArgumentNullException">userManager</exception>
        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        /// <summary>
        /// Gets a list of all the registered users.
        /// </summary>
        /// <returns>users</returns>
        [HttpGet("api/users")]
        public IList<UserInfo> Get()
        {
            return (from u in _userManager.Users.ToList()
                select new UserInfo(u)).ToList();
        }
    }
}
