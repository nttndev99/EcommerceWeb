
using Ecommerce.Application.DTOs.Product;
using Ecommerce.Application.Repositories.Interfaces;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetByProductIdAsync(int productId, CancellationToken ct = default);
        Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken ct = default); // ← Add this


    }

}