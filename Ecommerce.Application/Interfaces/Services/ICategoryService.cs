using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Category;

namespace Ecommerce.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<PagedResult<CategoryListDto>> GetPagedAsync(CategoryFilterParams filter, CancellationToken ct = default);
        Task<CategoryDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IEnumerable<CategoryDto>> GetAllActiveAsync(CancellationToken ct = default);
        Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default);
        Task<Result<CategoryDto>> UpdateAsync(UpdateCategoryDto dto, CancellationToken ct = default);
        Task<Result> SoftDeleteAsync(int id, CancellationToken ct = default);
        Task<Result> HardDeleteAsync(int id, CancellationToken ct = default);
        Task<Result<CategoryDto>> RestoreAsync(int id, CancellationToken ct = default);
        Task<PagedResult<CategoryListDto>> GetDeletedAsync(CategoryFilterParams filter, CancellationToken ct = default);
    }

}