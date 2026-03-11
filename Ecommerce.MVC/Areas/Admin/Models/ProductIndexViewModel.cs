
using System.ComponentModel.DataAnnotations;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Product;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ecommerce.MVC.Areas.Admin.Models
{
    // ===== Product View Models =====
    public class ProductIndexViewModel
    {
        public PagedResult<ProductListDto> Products { get; set; } = new();
        public ProductFilterParams Filter { get; set; } = new();
        public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Statuses { get; set; } = Enumerable.Empty<SelectListItem>();
        public int LowStockCount { get; set; }
    }

    public class ProductFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(300)]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Slug { get; set; }

        public string? Description { get; set; }

        [Display(Name = "Short Description")]
        [StringLength(500)]
        public string? ShortDescription { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative.")]
        [Display(Name = "Base Price")]
        [DataType(DataType.Currency)]
        public decimal BasePrice { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Sale Price")]
        [DataType(DataType.Currency)]
        public decimal? SalePrice { get; set; }

        [StringLength(100)]
        public string? SKU { get; set; }

        [Display(Name = "Status")]
        public Domain.Enums.ProductStatus Status { get; set; } = Domain.Enums.ProductStatus.Draft;

        [Display(Name = "Featured Product")]
        public bool IsFeatured { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [StringLength(200)]
        public string? Brand { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Weight { get; set; }

        [StringLength(500)]
        public string? Tags { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Statuses { get; set; } = Enumerable.Empty<SelectListItem>();
        public bool IsEdit => Id > 0;
    }

    public class ProductDetailViewModel
    {
        public ProductDto Product { get; set; } = new();
    }

}