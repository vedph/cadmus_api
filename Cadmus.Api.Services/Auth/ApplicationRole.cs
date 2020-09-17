using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Cadmus.Api.Services.Auth
{
    /// <summary>
    /// Application role.
    /// </summary>
    public sealed class ApplicationRole : MongoRole
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationRole"/> class.
        /// </summary>
        public ApplicationRole()
        {
            Claims = new List<IdentityRoleClaim<string>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationRole" /> class.
        /// </summary>
        /// <param name="name">The role name.</param>
        /// <exception cref="ArgumentNullException">name</exception>
        public ApplicationRole(string name)
        {
            Name = name ?? throw new System.ArgumentNullException(nameof(name));
            NormalizedName = name.ToUpperInvariant();
            Claims = new List<IdentityRoleClaim<string>>();
        }
    }
}
