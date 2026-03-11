using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Inventory;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IUnitOfWork _uow;
        public InventoryService(IUnitOfWork uow) => _uow = uow;

        public async Task<PagedResult<InventoryDto>> GetPagedAsync(InventoryFilterParams filter, CancellationToken ct = default)
        {
            var query = _uow.Inventories.Query()
                .Where(i => !i.IsDeleted && !i.Product.IsDeleted)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.ToLower();
                query = query.Where(i => i.Product.Name.ToLower().Contains(s) ||
                                        (i.ProductVariant != null && i.ProductVariant.Name.ToLower().Contains(s)));
            }

            if (filter.CategoryId.HasValue)
                query = query.Where(i => i.Product.CategoryId == filter.CategoryId.Value);

            if (filter.IsLowStock == true)
                query = query.Where(i => (i.Quantity - i.ReservedQuantity) <= i.LowStockThreshold && (i.Quantity - i.ReservedQuantity) > 0);

            if (filter.IsOutOfStock == true)
                query = query.Where(i => (i.Quantity - i.ReservedQuantity) <= 0);

            var totalCount = await query.CountAsync(ct);

            query = filter.SortBy?.ToLower() switch
            {
                "quantity" => filter.SortDirection == "desc" ? query.OrderByDescending(i => i.Quantity) : query.OrderBy(i => i.Quantity),
                _ => query.OrderBy(i => i.Product.Name)
            };

            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(i => new InventoryDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    ProductVariantId = i.ProductVariantId,
                    VariantName = i.ProductVariant != null ? i.ProductVariant.Name : null,
                    Quantity = i.Quantity,
                    ReservedQuantity = i.ReservedQuantity,
                    AvailableQuantity = i.Quantity - i.ReservedQuantity,
                    LowStockThreshold = i.LowStockThreshold,
                    WarehouseLocation = i.WarehouseLocation,
                    IsLowStock = (i.Quantity - i.ReservedQuantity) <= i.LowStockThreshold && (i.Quantity - i.ReservedQuantity) > 0,
                    IsOutOfStock = (i.Quantity - i.ReservedQuantity) <= 0,
                    LastStockUpdate = i.LastStockUpdate
                })
                .ToListAsync(ct);

            return PagedResult<InventoryDto>.Create(items, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<InventoryDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _uow.Inventories.Query()
                .Where(i => i.Id == id)
                .AsNoTracking()
                .Select(i => new InventoryDto
                {
                    Id = i.Id, ProductId = i.ProductId, ProductName = i.Product.Name,
                    ProductVariantId = i.ProductVariantId,
                    VariantName = i.ProductVariant != null ? i.ProductVariant.Name : null,
                    Quantity = i.Quantity, ReservedQuantity = i.ReservedQuantity,
                    AvailableQuantity = i.Quantity - i.ReservedQuantity,
                    LowStockThreshold = i.LowStockThreshold,
                    WarehouseLocation = i.WarehouseLocation,
                    IsLowStock = (i.Quantity - i.ReservedQuantity) <= i.LowStockThreshold,
                    IsOutOfStock = (i.Quantity - i.ReservedQuantity) <= 0,
                    LastStockUpdate = i.LastStockUpdate
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<Result<InventoryDto>> UpdateStockAsync(UpdateInventoryDto dto, CancellationToken ct = default)
        {
            var inventory = await _uow.Inventories.GetByIdAsync(dto.Id, ct);
            if (inventory == null) return Result<InventoryDto>.Failure("Inventory record not found.");

            inventory.Quantity = dto.Quantity;
            inventory.ReservedQuantity = dto.ReservedQuantity;
            inventory.LowStockThreshold = dto.LowStockThreshold;
            inventory.WarehouseLocation = dto.WarehouseLocation;
            inventory.LastStockUpdate = DateTime.UtcNow;
            inventory.UpdatedAt = DateTime.UtcNow;
            _uow.Inventories.Update(inventory);
            await _uow.SaveChangesAsync(ct);
            return Result<InventoryDto>.Success(await GetByIdAsync(dto.Id, ct) ?? new InventoryDto());
        }

        public async Task<IEnumerable<InventoryDto>> GetLowStockItemsAsync(CancellationToken ct = default)
        {
            return await _uow.Inventories.Query()
                .Where(i => !i.IsDeleted && !i.Product.IsDeleted && (i.Quantity - i.ReservedQuantity) <= i.LowStockThreshold)
                .AsNoTracking()
                .Select(i => new InventoryDto
                {
                    Id = i.Id, ProductId = i.ProductId, ProductName = i.Product.Name,
                    ProductVariantId = i.ProductVariantId,
                    VariantName = i.ProductVariant != null ? i.ProductVariant.Name : null,
                    Quantity = i.Quantity, ReservedQuantity = i.ReservedQuantity,
                    AvailableQuantity = i.Quantity - i.ReservedQuantity,
                    LowStockThreshold = i.LowStockThreshold,
                    IsLowStock = true,
                    IsOutOfStock = (i.Quantity - i.ReservedQuantity) <= 0,
                    LastStockUpdate = i.LastStockUpdate
                })
                .ToListAsync(ct);
        }
    }
}