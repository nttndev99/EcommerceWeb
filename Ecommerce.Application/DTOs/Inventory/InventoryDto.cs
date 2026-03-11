using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Application.Common;

namespace Ecommerce.Application.DTOs.Inventory
{
    // ===== Inventory =====
    public class InventoryDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int? ProductVariantId { get; set; }
        public string? VariantName { get; set; }
        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public int LowStockThreshold { get; set; }
        public string? WarehouseLocation { get; set; }
        public bool IsLowStock { get; set; }
        public bool IsOutOfStock { get; set; }
        public DateTime LastStockUpdate { get; set; }
    }

    public class UpdateInventoryDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }
        public int LowStockThreshold { get; set; }
        public string? WarehouseLocation { get; set; }
    }

    public class InventoryFilterParams : PaginationParams
    {
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public bool? IsLowStock { get; set; }
        public bool? IsOutOfStock { get; set; }
        public string SortBy { get; set; } = "ProductName";
        public string SortDirection { get; set; } = "asc";
    }
}