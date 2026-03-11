using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(EcommerceDbContext context) : base(context) { }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync(CancellationToken ct = default)
            => await _dbSet.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(ct);

        public async Task<Category?> GetWithChildrenAsync(int id, CancellationToken ct = default)
            => await _dbSet
                .Include(c => c.Children.Where(ch => !ch.IsDeleted))
                .FirstOrDefaultAsync(c => c.Id == id, ct);

        public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken ct = default)
            => await _dbSet.AnyAsync(c => c.Slug == slug && (!excludeId.HasValue || c.Id != excludeId.Value), ct);
    }
}