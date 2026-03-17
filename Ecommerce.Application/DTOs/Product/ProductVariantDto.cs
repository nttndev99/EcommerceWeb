using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Application.Common;

namespace Ecommerce.Application.DTOs.Product
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRODUCT VARIANT DTOs
    // ═══════════════════════════════════════════════════════════════════════════

    public class ProductVariantDto
    {
        public int      Id           { get; set; }
        public int      ProductId    { get; set; }
        public string   Name         { get; set; } = string.Empty;
        public string?  SKU          { get; set; }
        public decimal  Price        { get; set; }
        public decimal? SalePrice    { get; set; }
        public string?  Color        { get; set; }
        public string?  ColorHex      { get; set; }
        public string?  Size         { get; set; }
        public string?  Material     { get; set; }
        public string?  ImageUrl     { get; set; }
        public bool     IsActive     { get; set; }
        public int      DisplayOrder { get; set; }
        public int      Stock        { get; set; }
    }

    public class CreateProductVariantDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Product ID must be valid.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Variant name is required.")]
        [MaxLength(200, ErrorMessage = "Name must be at most 200 characters.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "SKU must be at most 100 characters.")]
        public string? SKU { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 999_999_999, ErrorMessage = "Price must be greater than 0.")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Range(0.01, 999_999_999, ErrorMessage = "Sale price must be greater than 0.")]
        [DataType(DataType.Currency)]
        public decimal? SalePrice { get; set; }

        [MaxLength(50, ErrorMessage = "Color must be at most 50 characters.")]
        public string? Color { get; set; }

        [MaxLength(50, ErrorMessage = "Size must be at most 50 characters.")]
        public string? Size { get; set; }

        [MaxLength(100, ErrorMessage = "Material must be at most 100 characters.")]
        public string? Material { get; set; }

        [Url(ErrorMessage = "Image URL must be a valid URL.")]
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        [Range(0, 9999)]
        public int DisplayOrder { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Initial stock cannot be negative.")]
        public int InitialStock { get; set; } = 0;
    }

    public class UpdateProductVariantDto : CreateProductVariantDto
    {
        [Required]
        public int Id { get; set; }
    }

    public class ProductVariantFilterParams : PaginationParams
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Product ID must be valid.")]
        public int ProductId { get; set; }

        [MaxLength(100)]
        public string? Search        { get; set; }
        public bool?   IsActive      { get; set; }
        public string  SortBy        { get; set; } = "DisplayOrder";
        public string  SortDirection { get; set; } = "asc";
    }
}