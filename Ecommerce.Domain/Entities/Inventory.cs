

using Ecommerce.Domain.Common;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities;

public class Inventory : BaseEntity
{
    public int ProductId { get; set; }
    public int? ProductVariantId { get; set; }
    public int Quantity { get; set; } = 0;
    public int ReservedQuantity { get; set; } = 0;
    public int LowStockThreshold { get; set; } = 5;
    public string? WarehouseLocation { get; set; }
    public DateTime LastStockUpdate { get; set; } = DateTime.UtcNow;

    // Computed
    public int AvailableQuantity => Quantity - ReservedQuantity;
    public bool IsLowStock => AvailableQuantity <= LowStockThreshold;
    public bool IsOutOfStock => AvailableQuantity <= 0;

    // Navigation
    public Product Product { get; set; } = null!;
    public ProductVariant? ProductVariant { get; set; }
}

