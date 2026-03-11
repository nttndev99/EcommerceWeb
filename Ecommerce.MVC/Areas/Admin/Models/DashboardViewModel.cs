
namespace Ecommerce.MVC.Areas.Admin.Models
{
    // ===== Dashboard =====
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalVariants { get; set; }
        public int TotalImages { get; set; }
        public int OutOfStockCount { get; set; }
        public int LowStockCount { get; set; }

        public List<ProductSummary> RecentProducts { get; set; } = new();
        public List<StatusCount> StatusBreakdown { get; set; } = new();
        public List<CatStat> TopCategories { get; set; } = new();
        public List<StockAlert> StockAlerts { get; set; } = new();
    }

    public class ProductSummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? SalePrice { get; set; }
        public Domain.Enums.ProductStatus Status { get; set; }
        public bool IsFeatured { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? PrimaryImage { get; set; }
        public int TotalStock { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class StatusCount
    {
        public Domain.Enums.ProductStatus Status { get; set; }
        public int Count { get; set; }
    }

    public class CatStat
    {
        public string Name { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public bool IsActive { get; set; }
    }

    public class StockAlert
    {
        public int InvId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? VariantName { get; set; }
        public int Available { get; set; }
        public bool IsOutOfStock { get; set; }
    }
}