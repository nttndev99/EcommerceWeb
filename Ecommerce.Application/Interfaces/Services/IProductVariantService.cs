using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Product;

namespace Ecommerce.Application.Interfaces.Services
{
    public interface IProductVariantService
    {
        Task<PagedResult<ProductVariantDto>> GetPagedByProductAsync(ProductVariantFilterParams filter, CancellationToken ct = default);
        Task<ProductVariantDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IEnumerable<ProductVariantDto>> GetByProductIdAsync(int productId, CancellationToken ct = default);
        Task<PagedResult<ProductVariantDto>> GetByProductIdPagedAsync(int productId, int pageNumber, int pageSize, CancellationToken ct = default);
        Task<Result<ProductVariantDto>> CreateAsync(CreateProductVariantDto dto, CancellationToken ct = default);
        Task<Result<ProductVariantDto>> UpdateAsync(UpdateProductVariantDto dto, CancellationToken ct = default);
        
        Task<IEnumerable<ProductVariantDto>> GetTrashedByProductIdAsync(int productId, CancellationToken ct = default);
        Task<Result> SoftDeleteAsync(int id, CancellationToken ct = default);
        Task<Result> RestoreAsync(int id, CancellationToken ct = default);
        Task<Result> HardDeleteAsync(int id, CancellationToken ct = default);
        Task<Result> EmptyTrashByProductAsync(int productId, CancellationToken ct = default);
    }    

}