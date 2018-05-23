namespace CadmusApi.Models
{
    /// <summary>
    /// User info.
    /// </summary>
    public sealed class UserInfo
    {
        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserInfo"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        public UserInfo(ApplicationUser user)
        {
            UserName = user.UserName;
            FirstName = user.FirstName;
            LastName = user.LastName;
        }
    }
}
