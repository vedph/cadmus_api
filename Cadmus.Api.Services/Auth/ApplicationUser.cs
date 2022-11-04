using AspNetCore.Identity.Mongo.Model;

namespace Cadmus.Api.Services.Auth
{
    /// <summary>
    /// Application user.
    /// </summary>
    public class ApplicationUser : MongoUser
    {
        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string? LastName { get; set; }
    }
}
