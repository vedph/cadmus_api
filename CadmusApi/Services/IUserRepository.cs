using System.Linq;
using Fusi.Tools.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using CadmusApi.Models;

namespace CadmusApi.Services
{
    /// <summary>
    /// User repository. This is a wrapper around the users managed by the framework,
    /// to allow browsing users and updating their attributes.
    /// </summary>
    /// <typeparam name="TUser">The type of the user.</typeparam>
    public interface IUserRepository<TUser> where TUser : class
    {
        /// <summary>
        /// Gets the users matching the specified filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        Task<PagedData<UserWithRoles<TUser>>> GetUsersAsync(UserFilterModel filter);

        /// <summary>
        /// Gets the user from his name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>user or null if not found</returns>
        Task<UserWithRoles<TUser>> GetUserAsync(string name);

        /// <summary>
        /// Gets the users from their names.
        /// </summary>
        /// <param name="names">The names.</param>
        /// <returns>users</returns>
        Task<IList<UserWithRoles<TUser>>> GetUsersFromNamesAsync(IList<string> names);

        /// <summary>
        /// Updates the user's editable attributes.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="roles">The roles to be set, or null
        /// to avoid setting roles.</param>
        Task UpdateUserAsync(TUser user, IList<string> roles);

        /// <summary>
        /// Adds the user to the specified roles.
        /// </summary>
        /// <param name="userName">The user identifier.</param>
        /// <param name="roles">The roles.</param>
        Task AddUserToRolesAsync(string userName, IList<string> roles);

        /// <summary>
        /// Removes the user from the specified roles.
        /// </summary>
        /// <param name="userName">The user identifier.</param>
        /// <param name="roles">The roles.</param>
        Task RemoveUserFromRolesAsync(string userName, IList<string> roles);
    }

    /// <summary>
    /// A wrapper for a user with its roles.
    /// </summary>
    /// <typeparam name="TUser">the type of the user</typeparam>
    public sealed class UserWithRoles<TUser> where TUser : class
    {
        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public TUser User { get; set; }

        /// <summary>
        /// Gets or sets the roles.
        /// </summary>
        public string[] Roles { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserWithRoles{T}"/> class.
        /// </summary>
        public UserWithRoles()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserWithRoles{T}"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="roles">The roles.</param>
        public UserWithRoles(TUser user, IEnumerable<string> roles)
        {
            if (roles == null) throw new ArgumentNullException(nameof(roles));

            User = user ?? throw new ArgumentNullException(nameof(user));
            Roles = roles.ToArray();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{User}: {string.Join(", ", Roles)}";
        }
    }
}
