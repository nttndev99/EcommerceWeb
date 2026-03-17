using Ecommerce.Application.DTOs.Inventory;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.MVC.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ecommerce.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "RequireAdmin")]

    public class InventoriesController : Controller
    {
        private readonly IInventoryService _inventoryService;
    
        public InventoriesController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }
    
        // ─────────────────────────────────────────────
        // INDEX – danh sách có lọc + phân trang
        // ─────────────────────────────────────────────
        public async Task<IActionResult> Index(InventoryFilterParams filter, CancellationToken ct)
        {
            var result = await _inventoryService.GetPagedAsync(filter, ct);

            ViewBag.TotalAll = await _inventoryService.CountAsync(new InventoryFilterParams { PageSize = int.MaxValue }, ct);
            ViewBag.TotalLowStock = await _inventoryService.CountAsync(new InventoryFilterParams { IsLowStock = true,  PageSize = int.MaxValue }, ct);
            ViewBag.TotalOutOfStock = await _inventoryService.CountAsync(new InventoryFilterParams { IsOutOfStock = true, PageSize = int.MaxValue }, ct);

            ViewBag.Filter = filter;
            return View(result);
        }
    
        // ─────────────────────────────────────────────
        // DETAILS
        // ─────────────────────────────────────────────
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var item = await _inventoryService.GetByIdAsync(id, ct);
            if (item is null) return NotFound();
            return View(item);
        }
    
        // ─────────────────────────────────────────────
        // UPDATE (GET)  – UpdateInventoryDto
        // ─────────────────────────────────────────────
        public async Task<IActionResult> Update(int id, CancellationToken ct)
        {
            var item = await _inventoryService.GetByIdAsync(id, ct);
            if (item is null) return NotFound();
    
            var dto = new UpdateInventoryDto
            {
                Id                  = item.Id,
                Quantity            = item.Quantity,
                ReservedQuantity    = item.ReservedQuantity,
                LowStockThreshold   = item.LowStockThreshold,
                WarehouseLocation   = item.WarehouseLocation,
            };
            ViewBag.ProductName = item.ProductName;
            return View(dto);
        }
    
        // UPDATE (POST)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateInventoryDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(dto);
    
            var result = await _inventoryService.UpdateStockAsync(dto, ct);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error!);
                return View(dto);
            }
    
            TempData["Success"] = "Inventory update successful.";
            return RedirectToAction(nameof(Index));
        }
    
        // ─────────────────────────────────────────────
        // ADJUST (GET)  – AdjustInventoryDto
        // ─────────────────────────────────────────────
        public async Task<IActionResult> Adjust(int id, CancellationToken ct)
        {
            var item = await _inventoryService.GetByIdAsync(id, ct);
            if (item is null) return NotFound();
    
            var dto = new AdjustInventoryDto { Id = item.Id };
            ViewBag.Inventory = item;
            return View(dto);
        }
    
        // ADJUST (POST)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Adjust(AdjustInventoryDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Inventory = await _inventoryService.GetByIdAsync(dto.Id, ct);
                return View(dto);
            }
    
            var result = await _inventoryService.AdjustAsynckAsync(dto, ct);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error!);
                ViewBag.Inventory = await _inventoryService.GetByIdAsync(dto.Id, ct);
                return View(dto);
            }
    
            TempData["Success"] = "Inventory adjustment successful.";
            return RedirectToAction(nameof(Index));
        }
    

        // ─────────────────────────────────────────────
        // LOW STOCK
        // ─────────────────────────────────────────────
        public async Task<IActionResult> LowStock(CancellationToken ct)
        {
            var items = await _inventoryService.GetLowStockItemsAsync(ct);
            return View(items);
        }



    }
    
}