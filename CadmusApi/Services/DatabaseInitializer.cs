using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using CadmusApi.Models;

namespace CadmusApi.Services
{
    /// <summary>
    /// Database initializer.
    /// </summary>
    /// <seealso cref="CadmusApi.Services.IDatabaseInitializer" />
    public sealed class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseInitializer"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="userManager">The user manager.</param>
        /// <param name="roleManager">The role manager.</param>
        public DatabaseInitializer(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Seeds the database.
        /// </summary>
        public async Task Seed()
        {
            await _context.Database.EnsureCreatedAsync();

            // users
            const string sEmail = "fake@nowhere.com";
            ApplicationUser user;
            if (await _userManager.FindByEmailAsync(sEmail) == null)
            {
                // use the create rather than addorupdate so can set password
                user = new ApplicationUser
                {
                    UserName = "zeus",
                    Email = sEmail,
                    EmailConfirmed = true,
                    FirstName = "John",
                    LastName = "Doe"
                };
                await _userManager.CreateAsync(user, "P4ssw0rd!");
            }

            user = await _userManager.FindByEmailAsync(sEmail);
            string roleName = "admin";
            if (await _roleManager.FindByNameAsync(roleName) == null)
                await _roleManager.CreateAsync(new IdentityRole { Name = roleName });

            if (!await _userManager.IsInRoleAsync(user, roleName))
                await _userManager.AddToRoleAsync(user, roleName);
        }
    }

    /// <summary>
    /// Database initializer.
    /// </summary>
    public interface IDatabaseInitializer
    {
        /// <summary>
        /// Seeds the database.
        /// </summary>
        Task Seed();
    }
}
