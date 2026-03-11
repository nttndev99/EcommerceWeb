using System.Threading.Tasks;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Inventory;
using Ecommerce.Application.DTOs.Product;

namespace Ecommerce.Application.Interfaces.Services
{

    public interface IInventoryService
    {
        Task<PagedResult<InventoryDto>> GetPagedAsync(InventoryFilterParams filter, CancellationToken ct = default);
        Task<InventoryDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Result<InventoryDto>> UpdateStockAsync(UpdateInventoryDto dto, CancellationToken ct = default);
        Task<IEnumerable<InventoryDto>> GetLowStockItemsAsync(CancellationToken ct = default);
    }
}