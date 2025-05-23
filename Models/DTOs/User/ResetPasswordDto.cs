﻿using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.User
{
    public class ResetPasswordDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
   ErrorMessage = "The password must contain at least one uppercase letter, one lowercase letter, one number and one symbol.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
