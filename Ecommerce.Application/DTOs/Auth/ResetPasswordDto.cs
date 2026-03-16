using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Application.DTOs.Auth
{
    public class ResetPasswordDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
    
        [Required]
        public string Token { get; set; } = string.Empty;
    
        [Required, MinLength(6), DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;
    
        [Required, Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}