using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Domain.Entities
{
    public class AppUser: IdentityUser
    {
        // ── Profile ───────────────────────────────────
        public string  FullName   { get; set; } = string.Empty;
        public string? AvatarUrl  { get; set; }
        public string? Gender     { get; set; }           // "Male" | "Female" | "Other"
        public DateTime? DateOfBirth { get; set; }
    
        // ── Address (default shipping) ────────────────
        public string? AddressLine { get; set; }
        public string? Ward        { get; set; }
        public string? District    { get; set; }
        public string? Province    { get; set; }
    
        // ── Status ────────────────────────────────────
        public bool     IsActive  { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
    
        // ── Navigation ────────────────────────────────
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}