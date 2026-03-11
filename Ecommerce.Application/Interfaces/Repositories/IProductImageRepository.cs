
using Ecommerce.Application.Repositories.Interfaces;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories
{
    public interface IProductImageRepository : IRepository<ProductImage>
    {
        Task<IEnumerable<ProductImage>> GetByProductIdAsync(int productId, CancellationToken ct = default);
        Task SetPrimaryImageAsync(int productId, int imageId, CancellationToken ct = default);
    }
}