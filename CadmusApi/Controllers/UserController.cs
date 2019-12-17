using Fusi.Tools.Data;
using CadmusApi.Models;
using CadmusApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CadmusApi.Controllers
{
    /// <summary>
    /// Users controller. This allows administrators to edit users.
    /// </summary>
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository<ApplicationUser> _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController" /> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <exception cref="ArgumentNullException">userManager</exception>
        public UserController(IUserRepository<ApplicationUser> repository)
        {
            _repository = repository;
        }

        private static UserModel UserToModel(ApplicationUser user, string[] roles)
        {
            return new UserModel
            {
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd?.UtcDateTime
            };
        }

        /// <summary>
        /// Gets the specified page from the list of registered users.
        /// Use page size=0 to get all the users at once.
        /// </summary>
        /// <param name="filter">The user's filter and paging data.</param>
        /// <returns>page of users list with <c>items</c> and <c>total</c></returns>
        [HttpGet("api/users")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Authorize]
        public async Task<ActionResult<DataPage<UserModel>>> GetUsers(
            [FromQuery] UserFilterModel filter)
        {
            if (!ModelState.IsValid) return BadRequest();

            DataPage<UserWithRoles<ApplicationUser>> page =
                await _repository.GetUsersAsync(filter);

            // prepare results (we return only a subset of user data)
            List<UserModel> results = new List<UserModel>();
            foreach (var ur in page.Items)
                results.Add(UserToModel(ur.User, ur.Roles));

            return Ok(new DataPage<UserModel>(
                filter.PageNumber, filter.PageSize, page.Total, results));
        }

        /// <summary>
        /// Gets the details about the user with the specified ID.
        /// </summary>
        /// <param name="name">The user name.</param>
        /// <returns>User with roles</returns>
        [HttpGet("api/users/{name}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<UserModel>> GetUser([FromRoute] string name)
        {
            UserWithRoles<ApplicationUser> user =
                await _repository.GetUserAsync(name);
            if (user == null) return NotFound();
            return Ok(UserToModel(user.User, user.Roles));
        }

        /// <summary>
        /// Gets the details about the current user.
        /// </summary>
        /// <returns>User with roles</returns>
        [HttpGet("api/user-info")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [Authorize]
        public async Task<ActionResult<UserModel>> GetCurrentUser()
        {
            UserWithRoles<ApplicationUser> user =
                await _repository.GetUserAsync(User.Identity.Name);
            if (user == null) return NotFound();
            return Ok(UserToModel(user.User, user.Roles));
        }

        /// <summary>
        /// Gets information about all the users whose names are specified.
        /// </summary>
        /// <param name="names">The names of the users to get information of,
        /// separated by commas.</param>
        /// <returns>array of users informations</returns>
        [HttpGet("api/users-from-names")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Authorize]
        public async Task<ActionResult<UserModel[]>> GetUsersFromNames(
            [FromQuery] string names)
        {
            string[] userNames = names.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (userNames.Length == 0) return Ok(new UserModel[0]);

            IList<UserWithRoles<ApplicationUser>> users =
                await _repository.GetUsersFromNamesAsync(userNames);

            // prepare results (we return only a subset of user data)
            List<UserModel> results = new List<UserModel>();
            foreach (var ur in users)
                results.Add(UserToModel(ur.User, ur.Roles));

            return Ok(results.ToArray());
        }

        /// <summary>
        /// Update the specified user data.
        /// </summary>
        /// <param name="model">The user data model.</param>
        [HttpPut("api/users")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> UpdateUser([FromBody] UserBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            await _repository.UpdateUserAsync(new ApplicationUser
            {
                UserName = model.UserName,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                EmailConfirmed = model.EmailConfirmed,
                LockoutEnabled = model.LockoutEnabled,
                LockoutEnd = model.LockoutEnd
            }, model.Roles);

            return Ok();
        }
    }
}
