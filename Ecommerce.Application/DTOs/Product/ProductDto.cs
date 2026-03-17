using System.ComponentModel.DataAnnotations;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Inventory;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.DTOs.Product;

public class ProductDto
{
    public int    Id               { get; set; }
    public string Name             { get; set; } = string.Empty;
    public string Slug             { get; set; } = string.Empty;
    public string? Description     { get; set; }
    public string? ShortDescription{ get; set; }
    public decimal  BasePrice      { get; set; }
    public decimal? SalePrice      { get; set; }
    public string?  SKU            { get; set; }
    public ProductStatus Status    { get; set; }
    public bool   IsFeatured       { get; set; }
    public int    CategoryId       { get; set; }
    public string CategoryName     { get; set; } = string.Empty;
    public string? Brand           { get; set; }
    public string? Tags            { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public int    TotalStock       { get; set; }
    public int    VariantCount     { get; set; }
    public DateTime CreatedAt      { get; set; }
    public List<ProductVariantDto> Variants    { get; set; } = new();
    public List<ProductImageDto>   Images      { get; set; } = new();
    public List<InventoryDto>      Inventories { get; set; } = new();
}

public class ProductListDto
{
    public int      Id             { get; set; }
    public string   Name           { get; set; } = string.Empty;
    public string   Slug           { get; set; } = string.Empty;
    public decimal  BasePrice      { get; set; }
    public decimal? SalePrice      { get; set; }
    public string?  SKU            { get; set; }
    public int      VariantCount   { get; set; }
    public ProductStatus Status    { get; set; }
    public bool     IsFeatured     { get; set; }
    public bool     IsNew     { get; set; }
    public string   CategoryName   { get; set; } = string.Empty;
    public string?  Brand          { get; set; }
    public string?  PrimaryImageUrl{ get; set; }
    public int      TotalStock     { get; set; }
    public DateTime CreatedAt      { get; set; }
}

// FILTER
public class ProductFilterParams : PaginationParams
{
    [MaxLength(100)]
    public string?        Search        { get; set; }
    public int?           CategoryId    { get; set; }
    public ProductStatus? Status        { get; set; }
    public bool?          IsFeatured    { get; set; }
    [MaxLength(100)]
    public string?        Brand         { get; set; }
    [Range(0, double.MaxValue)]
    public decimal?       MinPrice      { get; set; }
    [Range(0, double.MaxValue)]
    public decimal?       MaxPrice      { get; set; }
    public bool?          InStock       { get; set; }
    public string         SortBy        { get; set; } = "CreatedAt";
    public string         SortDirection { get; set; } = "desc";
}

// CREATE
public class CreateProductDto
{
    [Required(ErrorMessage = "Product name is required.")]
    [MaxLength(255, ErrorMessage = "Name must be at most 255 characters.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300, ErrorMessage = "Slug must be at most 300 characters.")]
    [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$",
        ErrorMessage = "Slug must be lowercase letters, numbers and hyphens only.")]
    public string? Slug { get; set; }

    [MaxLength(4000, ErrorMessage = "Description must be at most 4000 characters.")]
    public string? Description { get; set; }

    [MaxLength(500, ErrorMessage = "Short description must be at most 500 characters.")]
    public string? ShortDescription { get; set; }

    [Required(ErrorMessage = "Base price is required.")]
    [Range(0.01, 999_999_999, ErrorMessage = "Base price must be greater than 0.")]
    [DataType(DataType.Currency)]
    public decimal BasePrice { get; set; }

    [Range(0.01, 999_999_999, ErrorMessage = "Sale price must be greater than 0.")]
    [DataType(DataType.Currency)]
    public decimal? SalePrice { get; set; }

    [MaxLength(100, ErrorMessage = "SKU must be at most 100 characters.")]
    public string? SKU { get; set; }

    [EnumDataType(typeof(ProductStatus), ErrorMessage = "Invalid product status.")]
    public ProductStatus Status { get; set; } = ProductStatus.Draft;

    public bool IsFeatured { get; set; } = false;

    [Required(ErrorMessage = "Category is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category.")]
    public int CategoryId { get; set; }

    [MaxLength(100)]
    public string? Brand { get; set; }

    [Range(0.001, 9999, ErrorMessage = "Weight must be between 0.001 and 9999.")]
    public decimal? Weight { get; set; }

    [MaxLength(500, ErrorMessage = "Tags must be at most 500 characters.")]
    public string? Tags { get; set; }
}

public class UpdateProductDto : CreateProductDto
{
    [Required]
    public int Id { get; set; }
}



