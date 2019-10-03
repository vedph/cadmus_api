﻿using System.ComponentModel.DataAnnotations;

namespace CadmusApi.Models
{
    /// <summary>
    /// Reset password request binding model.
    /// </summary>
    public sealed class ResetPasswordRequestBindingModel
    {
        /// <summary>
        /// The email address.
        /// </summary>
        [Required]
        public string Email { get; set; }
    }
}
