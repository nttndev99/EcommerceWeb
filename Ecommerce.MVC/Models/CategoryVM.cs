using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Application.DTOs.Category;

namespace Ecommerce.MVC.Models
{
    public class CategoryVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }

        public int? ParentId { get; set; }
        public string? ParentName { get; set; }

        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public int ProductCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<CategoryVM> Children { get; set; } = new();
    }
}