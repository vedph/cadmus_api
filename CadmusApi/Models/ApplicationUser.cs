using AspNetCore.Identity.MongoDbCore.Models;
using System;

namespace CadmusApi.Models
{
    /// <summary>
    /// Application user.
    /// </summary>
    public class ApplicationUser : MongoIdentityUser<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationUser"/> class.
        /// </summary>
        public ApplicationUser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationUser"/> class.
        /// </summary>
        /// <param name="userName">The name of the user.</param>
        /// <param name="email">The email address of the user.</param>
        public ApplicationUser(string userName, string email) : base(userName, email)
        {
        }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string LastName { get; set; }
    }
}

/* MSSQL
using Microsoft.AspNetCore.Identity;

namespace CadmusApi.Models
{
    /// <summary>
    /// Profile data for application users.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Identity.IdentityUser" />
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string LastName { get; set; }
    }
}
*/
