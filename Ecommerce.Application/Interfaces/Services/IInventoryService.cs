using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Inventory;
using Ecommerce.Application.DTOs.Product;

namespace Ecommerce.Application.Interfaces;

public interface IInventoryService
{
    Task<PagedResult<InventoryDto>> GetPagedAsync(InventoryFilterParams filter, CancellationToken ct = default);
    Task<InventoryDto?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Lấy ProductDto kèm toàn bộ Inventories theo productId — dùng cho Inventories/Detail
    /// </summary>
    Task<ProductDto?> GetByProductIdAsync(int productId, CancellationToken ct = default);

    Task<Result<InventoryDto>> UpdateStockAsync(UpdateInventoryDto dto, CancellationToken ct = default);
    Task<Result<InventoryDto>> AdjustAsynckAsync(AdjustInventoryDto dto, CancellationToken ct = default);
    Task<IEnumerable<InventoryDto>> GetLowStockItemsAsync(CancellationToken ct = default);
    Task<int> CountAsync(InventoryFilterParams filter, CancellationToken ct = default);
}