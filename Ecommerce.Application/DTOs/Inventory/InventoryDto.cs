using System.ComponentModel.DataAnnotations;
using Ecommerce.Application.Common;

namespace Ecommerce.Application.DTOs.Inventory;

public class InventoryDto
{
    public int      Id                { get; set; }
    public int      ProductId         { get; set; }
    public string   ProductName       { get; set; } = string.Empty;
    public int?     ProductVariantId  { get; set; }
    public string?  VariantName       { get; set; }
    public int      Quantity          { get; set; }
    public int      ReservedQuantity  { get; set; }
    public int      AvailableQuantity { get; set; }
    public int      LowStockThreshold { get; set; }
    public string?  WarehouseLocation { get; set; }
    public bool     IsLowStock        { get; set; }
    public bool     IsOutOfStock      { get; set; }
    public DateTime LastStockUpdate   { get; set; }
}

// UPDATE
public class UpdateInventoryDto
{
    [Required]
    public int Id { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
    public int Quantity { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Reserved quantity cannot be negative.")]
    public int ReservedQuantity { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Low stock threshold cannot be negative.")]
    public int LowStockThreshold { get; set; }

    [MaxLength(100, ErrorMessage = "Warehouse location must be at most 100 characters.")]
    public string? WarehouseLocation { get; set; }
}



// FILTER
public class InventoryFilterParams : PaginationParams
{
    [MaxLength(100)]
    public string? Search       { get; set; }
    public int?    CategoryId   { get; set; }
    public bool?   IsLowStock   { get; set; }
    public bool?   IsOutOfStock { get; set; }
    public string  SortBy        { get; set; } = "ProductName";
    public string  SortDirection { get; set; } = "asc";
}