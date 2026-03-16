using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Application.DTOs.Admin
{
    public class AdminResetPasswordDto
    {
        [Required] public string Id { get; set; } = string.Empty;
    
        [Required, MinLength(6), DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;
    
        [Required, Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}