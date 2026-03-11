using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Product;

namespace Ecommerce.Application.Interfaces.Services
{
    public interface IProductVariantService
    {
        Task<PagedResult<ProductVariantDto>> GetPagedByProductAsync(ProductVariantFilterParams filter, CancellationToken ct = default);
        Task<ProductVariantDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Result<ProductVariantDto>> CreateAsync(CreateProductVariantDto dto, CancellationToken ct = default);
        Task<Result<ProductVariantDto>> UpdateAsync(UpdateProductVariantDto dto, CancellationToken ct = default);
        Task<Result> DeleteAsync(int id, CancellationToken ct = default);
    }

}