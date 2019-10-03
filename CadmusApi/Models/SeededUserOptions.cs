using System.Text;

namespace CadmusApi.Models
{
    /// <summary>
    /// Options read from configuration for the users seeded at startup,
    /// read from appsettings.json. Derive your application user options
    /// from this, when you add new properties to the default ones.
    /// </summary>
    public class SeededUserOptions
    {
        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password. Of course the password found in
        /// appsettings.json is fake; the real one is in the environment.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the roles.
        /// </summary>
        public string[] Roles { get; set; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(UserName);
            if (Roles?.Length > 0)
            {
                sb.Append(" - Roles: ").Append(string.Join(", ", Roles));
            }
            return sb.ToString();
        }
    }
}
