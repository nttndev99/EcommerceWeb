using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Application.DTOs.Admin
{
    public class AdminEditUserDto
    {
        [Required] public string  Id          { get; set; } = string.Empty;
        [Required, MaxLength(100)]
        public string  FullName    { get; set; } = string.Empty;
        [Phone]
        public string? PhoneNumber { get; set; }
        public bool    IsActive    { get; set; }
    }
}