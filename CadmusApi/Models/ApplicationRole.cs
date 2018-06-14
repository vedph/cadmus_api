using AspNetCore.Identity.MongoDbCore.Models;
using System;

namespace CadmusApi.Models
{
    /// <summary>
    /// Application role.
    /// </summary>
    public class ApplicationRole : MongoIdentityRole<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationRole"/> class.
        /// </summary>
        public ApplicationRole()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationRole"/> class.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        public ApplicationRole(string roleName) : base(roleName)
        {
        }
    }
}