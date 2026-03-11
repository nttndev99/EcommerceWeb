using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Application.Common;

namespace Ecommerce.Application.DTOs.Product
{
    // ===== ProductVariant =====
    public class ProductVariantDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? Material { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public int Stock { get; set; }
    }

    public class CreateProductVariantDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? Material { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
        public int InitialStock { get; set; } = 0;
    }

    public class UpdateProductVariantDto : CreateProductVariantDto
    {
        public int Id { get; set; }
    }

    public class ProductVariantFilterParams : PaginationParams
    {
        public int ProductId { get; set; }
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "DisplayOrder";
        public string SortDirection { get; set; } = "asc";
    }
}