using Ecommerce.Application.DTOs.Inventory;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.MVC.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ecommerce.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly ICategoryService _categoryService;

        public InventoryController(IInventoryService inventoryService, ICategoryService categoryService)
        {
            _inventoryService = inventoryService;
            _categoryService = categoryService;
        }

        // GET: /Inventory
        public async Task<IActionResult> Index([FromQuery] InventoryFilterParams filter, CancellationToken ct)
        {
            var paged = await _inventoryService.GetPagedAsync(filter, ct);
            var categories = await _categoryService.GetAllActiveAsync(ct);
            var lowStock = await _inventoryService.GetLowStockItemsAsync(ct);

            var vm = new InventoryIndexViewModel
            {
                Inventories = paged,
                Filter = filter,
                Categories = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString())),
                LowStockCount = lowStock.Count(i => !i.IsOutOfStock),
                OutOfStockCount = lowStock.Count(i => i.IsOutOfStock)
            };

            return View(vm);
        }

        // GET: /Inventory/Edit/5
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var inv = await _inventoryService.GetByIdAsync(id, ct);
            if (inv == null) return NotFound();
            return View(new InventoryEditViewModel
            {
                Id = inv.Id,
                ProductName = inv.ProductName,
                VariantName = inv.VariantName,
                Quantity = inv.Quantity,
                ReservedQuantity = inv.ReservedQuantity,
                LowStockThreshold = inv.LowStockThreshold,
                WarehouseLocation = inv.WarehouseLocation
            });
        }

        // POST: /Inventory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InventoryEditViewModel vm, CancellationToken ct)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var dto = new UpdateInventoryDto
            {
                Id = vm.Id,
                Quantity = vm.Quantity,
                ReservedQuantity = vm.ReservedQuantity,
                LowStockThreshold = vm.LowStockThreshold,
                WarehouseLocation = vm.WarehouseLocation
            };

            var result = await _inventoryService.UpdateStockAsync(dto, ct);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error!);
                return View(vm);
            }

            TempData["Success"] = "Inventory updated.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Inventory/LowStock
        public async Task<IActionResult> LowStock(CancellationToken ct)
        {
            var items = await _inventoryService.GetLowStockItemsAsync(ct);
            return View(items);
        }
    }
}