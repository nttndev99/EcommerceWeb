using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Application.DTOs.Admin
{
    public class RoleDto
    {
        public string Id          { get; set; } = string.Empty;
        public string Name        { get; set; } = string.Empty;
        public int    UserCount   { get; set; }
    }
    
    public class CreateRoleDto
    {
        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;
    }
    
    public class AssignRoleDto
    {
        [Required] public string UserId   { get; set; } = string.Empty;
        [Required] public string RoleName { get; set; } = string.Empty;
    }
}