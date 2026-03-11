
using Ecommerce.Domain.Common;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities;

/// <summary>
/// Sản phẩm gốc (parent product — không chứa giá/tồn kho trực tiếp)
///
/// RELATIONSHIPS:
///   Category      (1) ────< Product       (N)   [CategoryId FK]
///   Product       (1) ────< ProductVariant(N)   [ProductId FK]
///   Product       (1) ────< ProductImage  (N)   [ProductId FK]
///
/// Giá / Tồn kho được quản lý ở ProductVariant (không phải Product)
/// </summary>
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
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

    // Navigation
    public Category Category { get; set; } = null!;
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
}
