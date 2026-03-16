using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Application.DTOs.Admin
{
    public class AdminCreateUserDto
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
    
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    
        [Phone]
        public string? PhoneNumber { get; set; }
    
        [Required, MinLength(6), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    
        [Required, Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    
        [Required]
        public string Role { get; set; } = "Customer";
    
        public bool IsActive { get; set; } = true;
    
        public bool EmailConfirmed { get; set; } = true;
    }
}