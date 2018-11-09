namespace CadmusApi.Models
{
    /// <summary>
    /// User.
    /// </summary>
    public sealed class UserModel
    {
        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the user's email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the roles.
        /// </summary>
        public string[] Roles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this user is verified.
        /// </summary>
        public bool IsVerified { get; set; }

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
