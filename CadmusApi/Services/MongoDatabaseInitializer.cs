using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using CadmusApi.Models;
using Microsoft.Extensions.Configuration;
using System;

namespace CadmusApi.Services
{
    /// <summary>
    /// MongoDB database initializer.
    /// </summary>
    /// <seealso cref="CadmusApi.Services.IDatabaseInitializer" />
    public sealed class MongoDatabaseInitializer : IDatabaseInitializer
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDatabaseInitializer" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="userManager">The user manager.</param>
        /// <param name="roleManager">The role manager.</param>
        public MongoDatabaseInitializer(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        private async Task SeedRoles()
        {
            foreach (string role in new[] { "admin" })
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new ApplicationRole
                    {
                        Name = role
                    });
                }
            }
        }

        /// <summary>
        /// Seeds the database.
        /// </summary>
        /// <param name="provider">The provider.</param>
        public async Task SeedAsync(IServiceProvider provider)
        {
            Serilog.Log.Information("Seeding users");

            await SeedRoles();

            const string email = "dfusi@hotmail.com";
            ApplicationUser user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                IConfigurationSection section = _configuration.GetSection("Admin");
                user = new ApplicationUser
                {
                    UserName = section["UserName"],
                    Email = section["Email"],
                    EmailConfirmed = true,
                    FirstName = section["FirstName"],
                    LastName = section["LastName"]
                };
                await _userManager.CreateAsync(user, section["Password"]);
            }

            if (!await _userManager.IsInRoleAsync(user, "admin"))
                await _userManager.AddToRoleAsync(user, "admin");
        }
    }
}