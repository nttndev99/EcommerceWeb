
using Ecommerce.Application.Repositories.Interfaces;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories
{
    public interface IInventoryRepository : IRepository<Inventory>
    {
        Task<Inventory?> GetByProductAndVariantAsync(int productId, int? variantId, CancellationToken ct = default);
        Task<IEnumerable<Inventory>> GetLowStockItemsAsync(CancellationToken ct = default);
    }
}