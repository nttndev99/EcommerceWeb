using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Application.DTOs.Admin
{
    public class AdminUserListDto
    {
        public string       Id             { get; set; } = string.Empty;
        public string       FullName       { get; set; } = string.Empty;
        public string       Email          { get; set; } = string.Empty;
        public string?      PhoneNumber    { get; set; }
        public string       AvatarUrl      { get; set; } = string.Empty;
        public bool         IsActive       { get; set; }
        public bool         EmailConfirmed { get; set; }
        public bool         IsLockedOut    { get; set; }
        public DateTime     CreatedAt      { get; set; }
        public DateTime?    LastLoginAt    { get; set; }
        public List<string> Roles          { get; set; } = new();
    }
}