using Ecommerce.Application.Repositories.Interfaces;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories
{

    

    public interface IProductVariantRepository : IRepository<ProductVariant>
    {
        Task<IEnumerable<ProductVariant>> GetByProductIdAsync(int productId, CancellationToken ct = default);
        Task<bool> SKUExistsAsync(string sku, int? excludeId = null, CancellationToken ct = default);
    }




}