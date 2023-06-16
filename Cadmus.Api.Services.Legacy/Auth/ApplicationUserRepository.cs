using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusi.Tools.Data;
using Microsoft.AspNetCore.Identity;

namespace Cadmus.Api.Services.Auth;

/// <summary>
/// ASP.NET Core users repository.
/// </summary>
public sealed class ApplicationUserRepository : IUserRepository<ApplicationUser>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationUserRepository"/> class.
    /// </summary>
    /// <param name="userManager">The user manager.</param>
    /// <param name="roleManager">The role manager.</param>
    public ApplicationUserRepository(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
    }

    private async Task<UserWithRoles<ApplicationUser>> GetUserWithRolesAsync(ApplicationUser user)
    {
        IList<string> roles = await _userManager.GetRolesAsync(user);
        return new UserWithRoles<ApplicationUser>(user, roles);
    }

    /// <summary>
    /// Gets the user from his name.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>user or null if not found</returns>
    /// <exception cref="ArgumentNullException">null name</exception>
    public async Task<UserWithRoles<ApplicationUser>?> GetUserAsync(string name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));

        ApplicationUser? user = await _userManager.FindByNameAsync(name);
        if (user == null) return null;
        return await GetUserWithRolesAsync(user);
    }

    /// <summary>
    /// Gets the users.
    /// </summary>
    /// <param name="filter">The filter. Use page size=0 to get all the users.</param>
    /// <returns>users page</returns>
    /// <exception cref="ArgumentNullException">filter</exception>
    public async Task<DataPage<UserWithRoles<ApplicationUser>>>
        GetUsersAsync(UserFilterModel filter)
    {
        if (filter == null) throw new ArgumentNullException(nameof(filter));

        IQueryable<ApplicationUser> users = _userManager.Users;

        if (!string.IsNullOrEmpty(filter.Name))
        {
            users = users.Where(u => u.UserName!.Contains(filter.Name) ||
                u.LastName!.Contains(filter.Name) ||
                u.FirstName!.Contains(filter.Name));
        }

        int total = users.Count();
        users = users.OrderBy(u => u.UserName);

        List<UserWithRoles<ApplicationUser>> results = new();
        foreach (ApplicationUser user in users.Skip(filter.GetSkipCount())
            .Take(filter.PageSize == 0 ? total : filter.PageSize))
        {
            UserWithRoles<ApplicationUser> result = await GetUserWithRolesAsync(user);
            results.Add(result);
        }

        return new DataPage<UserWithRoles<ApplicationUser>>(
            filter.PageNumber, filter.PageSize, total, results);
    }

    /// <summary>
    /// Gets the users from their names.
    /// </summary>
    /// <param name="names">The names.</param>
    /// <returns>users</returns>
    /// <exception cref="ArgumentNullException">names</exception>
    public async Task<IList<UserWithRoles<ApplicationUser>>>
        GetUsersFromNamesAsync(IList<string> names)
    {
        if (names == null) throw new ArgumentNullException(nameof(names));

        List<UserWithRoles<ApplicationUser>> results = new();
        foreach (string name in names)
        {
            ApplicationUser? user = await _userManager.FindByNameAsync(name);
            if (user != null) results.Add(await GetUserWithRolesAsync(user));
        }
        return results;
    }

    /// <summary>
    /// Updates the user's editable attributes.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="roles">The roles to be set, or null to avoid setting
    /// roles.</param>
    /// <exception cref="ArgumentNullException">user</exception>
    public async Task UpdateUserAsync(ApplicationUser user,
        IList<string>? roles)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        ApplicationUser? old = await _userManager.FindByNameAsync(user.UserName!);
        if (old == null) return;

        old.FirstName = user.FirstName;
        old.LastName = user.LastName;
        old.Email = user.Email;
        old.NormalizedEmail = user.Email!.ToUpperInvariant();
        old.EmailConfirmed = user.EmailConfirmed;
        old.LockoutEnabled = user.LockoutEnabled;
        old.LockoutEnd = user.LockoutEnd;

        await _userManager.UpdateAsync(old);

        // roles
        if (roles != null)
        {
            HashSet<string> oldRoles = new(await _userManager.GetRolesAsync(old));
            HashSet<string> newRoles = new(roles);

            var removed = oldRoles.Except(newRoles).ToList();
            if (removed.Count > 0) await _userManager.RemoveFromRolesAsync(old, removed);

            var added = newRoles.Except(oldRoles).ToList();
            if (added.Count > 0)
            {
                // ensure that all the roles we are adding do exist
                foreach (string role in added)
                {
                    if (await _roleManager.RoleExistsAsync(role)) continue;
                    await _roleManager.CreateAsync(new ApplicationRole(role));
                }

                // add roles to user
                await _userManager.AddToRolesAsync(old, added);
            }
        }
    }

    /// <summary>
    /// Adds the user to the specified roles.
    /// </summary>
    /// <param name="userName">The user name.</param>
    /// <param name="roles">The roles.</param>
    public async Task AddUserToRolesAsync(string userName, IList<string> roles)
    {
        if (userName == null) throw new ArgumentNullException(nameof(userName));
        if (roles == null) throw new ArgumentNullException(nameof(roles));

        ApplicationUser? user = await _userManager.FindByNameAsync(userName);
        if (user == null) return;

        await _userManager.AddToRolesAsync(user, roles);
    }

    /// <summary>
    /// Removes the user from the specified roles.
    /// </summary>
    /// <param name="userName">The user name.</param>
    /// <param name="roles">The roles.</param>
    public async Task RemoveUserFromRolesAsync(string userName, IList<string> roles)
    {
        if (userName == null) throw new ArgumentNullException(nameof(userName));
        if (roles == null) throw new ArgumentNullException(nameof(roles));

        ApplicationUser? user = await _userManager.FindByNameAsync(userName);
        if (user == null) return;

        await _userManager.RemoveFromRolesAsync(user, roles);
    }
}
