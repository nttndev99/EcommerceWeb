
using Ecommerce.Domain.Enums;
using Ecommerce.Infrastructure.Persistence;
using Ecommerce.MVC.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly EcommerceDbContext _db;

        public DashboardController(EcommerceDbContext db) => _db = db;

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var vm = new DashboardViewModel
            {
                TotalProducts = await _db.Products.CountAsync(ct),
                ActiveProducts = await _db.Products.CountAsync(p => p.Status == ProductStatus.Active, ct),
                TotalCategories = await _db.Categories.CountAsync(ct),
                TotalVariants = await _db.ProductVariants.CountAsync(ct),
                TotalImages = await _db.ProductImages.CountAsync(ct),

                OutOfStockCount = await _db.Inventories
                    .CountAsync(i => (i.Quantity - i.ReservedQuantity) <= 0, ct),
                LowStockCount = await _db.Inventories
                    .CountAsync(i => (i.Quantity - i.ReservedQuantity) > 0
                                && (i.Quantity - i.ReservedQuantity) <= i.LowStockThreshold, ct),

                RecentProducts = await _db.Products
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(8)
                    .Select(p => new ProductSummary
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Brand = p.Brand,
                        BasePrice = p.BasePrice,
                        SalePrice = p.SalePrice,
                        Status = p.Status,
                        IsFeatured = p.IsFeatured,
                        CategoryName = p.Category.Name,
                        PrimaryImage = p.Images.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault(),
                        TotalStock = p.Inventories.Sum(i => i.Quantity - i.ReservedQuantity),
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync(ct),

                StatusBreakdown = await _db.Products
                    .GroupBy(p => p.Status)
                    .Select(g => new StatusCount { Status = g.Key, Count = g.Count() })
                    .OrderBy(s => s.Status)
                    .ToListAsync(ct),

                TopCategories = await _db.Categories
                    .Where(c => c.ParentId != null)
                    .Select(c => new CatStat
                    {
                        Name = c.Name,
                        ProductCount = c.Products.Count(p => !p.IsDeleted),
                        IsActive = c.IsActive
                    })
                    .OrderByDescending(c => c.ProductCount)
                    .Take(8)
                    .ToListAsync(ct),

                StockAlerts = await _db.Inventories
                    .Include(i => i.Product)
                    .Include(i => i.ProductVariant)
                    .Where(i => (i.Quantity - i.ReservedQuantity) <= i.LowStockThreshold)
                    .OrderBy(i => i.Quantity - i.ReservedQuantity)
                    .Take(10)
                    .Select(i => new StockAlert
                    {
                        InvId = i.Id,
                        ProductId = i.ProductId,
                        ProductName = i.Product.Name,
                        VariantName = i.ProductVariant != null ? i.ProductVariant.Name : null,
                        Available = i.Quantity - i.ReservedQuantity,
                        IsOutOfStock = (i.Quantity - i.ReservedQuantity) <= 0
                    })
                    .ToListAsync(ct),
            };

            return View(vm);
        }

    }
}