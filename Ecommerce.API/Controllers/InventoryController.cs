using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Application.DTOs.Inventory;
using Ecommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    // ═══════════════════════════════════════════════════════════════
    // INVENTORY
    // ═══════════════════════════════════════════════════════════════
    public class InventoryController : BaseApiController
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
            => _inventoryService = inventoryService;

        [HttpGet]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> GetAll(
            [FromQuery] InventoryFilterParams filter, CancellationToken ct)
        {
            var result = await _inventoryService.GetPagedAsync(filter, ct);
            return Paged(result);
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var inv = await _inventoryService.GetByIdAsync(id, ct);
            return inv is null ? NotFound($"Inventory {id} not found.") : Ok(inv);
        }

        [HttpGet("low-stock")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> LowStock(
            [FromQuery] InventoryFilterParams filter, CancellationToken ct)
        {
            filter.IsLowStock = true;
            var result = await _inventoryService.GetPagedAsync(filter, ct);
            return Paged(result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Update(
            int id, [FromBody] UpdateInventoryDto dto, CancellationToken ct)
        {
            dto.Id = id;
            var result = await _inventoryService.UpdateStockAsync(dto, ct);
            return FromResult(result, "Inventory updated.");
        }

        [HttpPost("{id:int}/adjust")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Adjust(
            int id, [FromBody] AdjustInventoryDto dto, CancellationToken ct)
        {
            dto.Id = id;
            var result = await _inventoryService.AdjustAsynckAsync(dto, ct);
            return FromResult(result, "Inventory adjusted.");
        }
    }
}