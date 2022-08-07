using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Cadmus.Api.Services.Auth;

namespace Cadmus.Api.Services.Seeding
{
    /// <summary>
    /// Application's user accounts database initializer.
    /// </summary>
    public sealed class ApplicationDatabaseInitializer
    {
        private readonly ILogger _logger;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationSeededUserOptions[] _seededUsersOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDatabaseInitializer" />
        /// class.
        /// </summary>
        public ApplicationDatabaseInitializer(IServiceProvider serviceProvider)
        {
            IConfiguration configuration =
                serviceProvider.GetService<IConfiguration>();

            ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger<ApplicationDatabaseInitializer>();

            _userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
            _roleManager = serviceProvider.GetService<RoleManager<ApplicationRole>>();

            _seededUsersOptions = configuration
                .GetSection("StockUsers")
                .Get<ApplicationSeededUserOptions[]>();
        }

        private async Task SeedRoles()
        {
            foreach (ApplicationSeededUserOptions options in _seededUsersOptions
                .Where(o => o.Roles != null))
            {
                foreach (string roleName in options.Roles)
                {
                    // add role if not existing
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _roleManager.CreateAsync(new ApplicationRole
                        {
                            Name = roleName
                        });
                    }
                }
            }
        }

        private async Task SeedUserAsync(ApplicationSeededUserOptions options)
        {
            ApplicationUser user =
                await _userManager.FindByNameAsync(options.UserName);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = options.UserName,
                    Email = options.Email,
                    // email is automatically confirmed for a stock user
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    FirstName = options.FirstName,
                    LastName = options.LastName
                };
                IdentityResult result =
                    await _userManager.CreateAsync(user, options.Password);
                if (!result.Succeeded)
                {
                    _logger.LogError(result.ToString());
                    return;
                }
                user = await _userManager.FindByNameAsync(options.UserName);
            }
            // ensure that user is automatically confirmed
            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            if (options.Roles != null)
            {
                foreach (string role in options.Roles)
                {
                    if (!await _userManager.IsInRoleAsync(user, role))
                        await _userManager.AddToRoleAsync(user, role);
                }
            }
        }

        private async Task SeedUsersWithRoles()
        {
            // roles
            await SeedRoles();

            // users
            if (_seededUsersOptions != null)
            {
                foreach (ApplicationSeededUserOptions options in _seededUsersOptions)
                    await SeedUserAsync(options);
            }
        }

        /// <summary>
        /// Seeds the database.
        /// </summary>
        public async Task SeedAsync()
        {
            await SeedUsersWithRoles();
        }
    }
}
