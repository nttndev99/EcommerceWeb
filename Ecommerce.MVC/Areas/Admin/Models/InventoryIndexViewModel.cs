
using System.ComponentModel.DataAnnotations;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Inventory;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ecommerce.MVC.Areas.Admin.Models
{
    // ===== Inventory View Models =====
    public class InventoryIndexViewModel
    {
        public PagedResult<InventoryDto> Inventories { get; set; } = new();
        public InventoryFilterParams Filter { get; set; } = new();
        public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
    }

    public class InventoryEditViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? VariantName { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, int.MaxValue)]
        [Display(Name = "Reserved Quantity")]
        public int ReservedQuantity { get; set; }

        [Range(0, int.MaxValue)]
        [Display(Name = "Low Stock Threshold")]
        public int LowStockThreshold { get; set; } = 5;

        [Display(Name = "Warehouse Location")]
        [StringLength(200)]
        public string? WarehouseLocation { get; set; }
    }
}