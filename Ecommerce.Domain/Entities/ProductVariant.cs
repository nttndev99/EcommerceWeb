using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class ProductVariant : BaseEntity
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;  // e.g. "Red / XL"
    public string? SKU { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public string? Color { get; set; }
    public string? ColorHex { get; set; }  // e.g. "#1a1a1a"
    public string? Size { get; set; }
    public string? Material { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;

    // Navigation
    public Product Product { get; set; } = null!;
    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
}
