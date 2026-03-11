
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Repositories
{
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {
        public ProductImageRepository(EcommerceDbContext context) : base(context) { }

        public async Task<IEnumerable<ProductImage>> GetByProductIdAsync(int productId, CancellationToken ct = default)
            => await _dbSet.Where(i => i.ProductId == productId).OrderBy(i => i.DisplayOrder).ToListAsync(ct);

        public async Task SetPrimaryImageAsync(int productId, int imageId, CancellationToken ct = default)
        {
            // Efficient batch update
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE ProductImages SET IsPrimary = 0 WHERE ProductId = {productId}", ct);
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE ProductImages SET IsPrimary = 1 WHERE Id = {imageId}", ct);
        }
    }
}