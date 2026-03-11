
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Repositories
{
    public class InventoryRepository : Repository<Domain.Entities.Inventory>, IInventoryRepository
    {
        public InventoryRepository(EcommerceDbContext context) : base(context) { }

        public async Task<Domain.Entities.Inventory?> GetByProductAndVariantAsync(int productId, int? variantId, CancellationToken ct = default)
            => await _dbSet.FirstOrDefaultAsync(i => i.ProductId == productId && i.ProductVariantId == variantId, ct);

        public async Task<IEnumerable<Domain.Entities.Inventory>> GetLowStockItemsAsync(CancellationToken ct = default)
            => await _dbSet
                .Include(i => i.Product)
                .Where(i => (i.Quantity - i.ReservedQuantity) <= i.LowStockThreshold)
                .ToListAsync(ct);
    }
}