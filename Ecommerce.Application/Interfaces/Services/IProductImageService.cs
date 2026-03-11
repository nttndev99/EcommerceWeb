using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Product;

namespace Ecommerce.Application.Interfaces.Services
{
    public interface IProductImageService
    {
        Task<IEnumerable<ProductImageDto>> GetByProductIdAsync(int productId, CancellationToken ct = default);
        Task<Result<ProductImageDto>> CreateAsync(CreateProductImageDto dto, CancellationToken ct = default);
        Task<Result<ProductImageDto>> UpdateAsync(UpdateProductImageDto dto, CancellationToken ct = default);
        Task<Result> DeleteAsync(int id, CancellationToken ct = default);
        Task<Result> SetPrimaryAsync(int productId, int imageId, CancellationToken ct = default);
    }

}