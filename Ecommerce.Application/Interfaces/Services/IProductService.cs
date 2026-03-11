using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Product;

namespace Ecommerce.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<PagedResult<ProductListDto>> GetPagedAsync(ProductFilterParams filter, CancellationToken ct = default);
        Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Result<ProductDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default);
        Task<Result<ProductDto>> UpdateAsync(UpdateProductDto dto, CancellationToken ct = default);
        Task<Result> DeleteAsync(int id, CancellationToken ct = default);
    }

}