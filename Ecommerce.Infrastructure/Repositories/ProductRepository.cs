using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(EcommerceDbContext context) : base(context) { }

        public async Task<Product?> GetWithDetailsAsync(int id, CancellationToken ct = default)
            => await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Variants.Where(v => v.IsActive))          // ← IsActive
                .Include(p => p.Images.Where(i => !i.IsDeleted))
                .Include(p => p.Inventories.Where(i => !i.IsDeleted))
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct); // ← thêm !p.IsDeleted
        public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken ct = default)
            => await _dbSet.AnyAsync(p => p.Slug == slug && (!excludeId.HasValue || p.Id != excludeId.Value), ct);

        public async Task<IEnumerable<Product>> GetByProductIdAsync(int productId, CancellationToken ct = default)
            => await _dbSet.Where(p => p.Id == productId).ToListAsync(ct);



    }
}