using System.ComponentModel.DataAnnotations;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Inventory;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.DTOs.Product;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public decimal BasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public string? SKU { get; set; }
    public ProductStatus Status { get; set; }
    public bool IsFeatured { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Tags { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public int TotalStock { get; set; }
    public int VariantCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProductVariantDto> Variants { get; set; } = new();
    public List<ProductImageDto> Images { get; set; } = new();
    public List<InventoryDto> Inventories { get; set; } = new();
}

public class ProductListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public string? SKU { get; set; }
    public ProductStatus Status { get; set; }
    public bool IsFeatured { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public int TotalStock { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProductFilterParams : PaginationParams
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public ProductStatus? Status { get; set; }
    public bool? IsFeatured { get; set; }
    public string? Brand { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStock { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public string SortDirection { get; set; } = "desc";
}

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public decimal BasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public string? SKU { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    public bool IsFeatured { get; set; } = false;
    public int CategoryId { get; set; }
    public string? Brand { get; set; }
    public decimal? Weight { get; set; }
    public string? Tags { get; set; }
}

public class UpdateProductDto : CreateProductDto
{
    public int Id { get; set; }
}