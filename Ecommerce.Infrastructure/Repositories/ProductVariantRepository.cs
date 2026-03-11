
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Repositories
{
    public class ProductVariantRepository : Repository<ProductVariant>, IProductVariantRepository
    {
        public ProductVariantRepository(EcommerceDbContext context) : base(context) { }

        public async Task<IEnumerable<ProductVariant>> GetByProductIdAsync(int productId, CancellationToken ct = default)
            => await _dbSet.Where(v => v.ProductId == productId).OrderBy(v => v.DisplayOrder).ToListAsync(ct);

        public async Task<bool> SKUExistsAsync(string sku, int? excludeId = null, CancellationToken ct = default)
            => await _dbSet.AnyAsync(v => v.SKU == sku && (!excludeId.HasValue || v.Id != excludeId.Value), ct);
    }
}