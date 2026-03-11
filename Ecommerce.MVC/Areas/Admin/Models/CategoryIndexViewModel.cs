
using System.ComponentModel.DataAnnotations;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Category;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ecommerce.MVC.Areas.Admin.Models.ViewModels
{
    // ===== Category View Models =====
    public class CategoryIndexViewModel
    {
        public PagedResult<CategoryListDto> PagedCategories { get; set; } = new();
        public CategoryFilterParams Filter { get; set; } = new();
        public IEnumerable<SelectListItem> ParentCategories { get; set; } = Enumerable.Empty<SelectListItem>();
    }

    public class CategoryFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(200, ErrorMessage = "Name must not exceed 200 characters.")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        [RegularExpression(@"^[a-z0-9\-]*$", ErrorMessage = "Slug must contain only lowercase letters, numbers and hyphens.")]
        public string? Slug { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [Display(Name = "Image URL")]
        [Url(ErrorMessage = "Please enter a valid URL.")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Parent Category")]
        public int? ParentId { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Display Order")]
        [Range(0, 9999)]
        public int DisplayOrder { get; set; } = 0;

        public IEnumerable<SelectListItem> ParentCategories { get; set; } = Enumerable.Empty<SelectListItem>();
        public bool IsEdit => Id > 0;
    }
}