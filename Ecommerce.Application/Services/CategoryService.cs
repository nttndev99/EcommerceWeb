using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Category;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _uow;

    public CategoryService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<PagedResult<CategoryListDto>> GetPagedAsync(CategoryFilterParams filter, CancellationToken ct = default)
    {
        var query = _uow.Categories.Query()
            .Where(c => !c.IsDeleted)
            .AsNoTracking();

        // Search
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(search) ||
                                    c.Slug.ToLower().Contains(search));
        }

        // Filters
        if (filter.IsActive.HasValue)
            query = query.Where(c => c.IsActive == filter.IsActive.Value);

        if (filter.ParentId.HasValue)
            query = query.Where(c => c.ParentId == filter.ParentId.Value);

        // Total count (performance: count before projection)
        var totalCount = await query.CountAsync(ct);

        // Sorting
        query = filter.SortBy?.ToLower() switch
        {
            "name" => filter.SortDirection == "desc" ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "displayorder" => filter.SortDirection == "desc" ? query.OrderByDescending(c => c.DisplayOrder) : query.OrderBy(c => c.DisplayOrder),
            "createdat" => filter.SortDirection == "desc" ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => query.OrderBy(c => c.Name)
        };

        // Pagination + Projection (single query, no N+1)
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(c => new CategoryListDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                IsActive = c.IsActive,
                ParentId = c.ParentId,
                ParentName = c.Parent != null ? c.Parent.Name : null,
                DisplayOrder = c.DisplayOrder,
                ProductCount = c.Products.Count(p => !p.IsDeleted),
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(ct);

        return PagedResult<CategoryListDto>.Create(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<CategoryDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _uow.Categories.Query()
            .Where(c => c.Id == id && !c.IsDeleted)
            .AsNoTracking()
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                ParentId = c.ParentId,
                ParentName = c.Parent != null ? c.Parent.Name : null,
                IsActive = c.IsActive,
                DisplayOrder = c.DisplayOrder,
                ProductCount = c.Products.Count(p => !p.IsDeleted),
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<CategoryDto>> GetAllActiveAsync(CancellationToken ct = default)
    {
        return await _uow.Categories.Query()
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                ParentId = c.ParentId,
                IsActive = c.IsActive,
                DisplayOrder = c.DisplayOrder,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(ct);
    }

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default)
    {
        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? SlugHelper.GenerateSlug(dto.Name)
            : dto.Slug;

        if (await _uow.Categories.SlugExistsAsync(slug, null, ct))
            return Result<CategoryDto>.Failure($"Slug '{slug}' already exists.");

        var category = new Category
        {
            Name = dto.Name,
            Slug = slug,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            ParentId = dto.ParentId,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder
        };

        await _uow.Categories.AddAsync(category, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<CategoryDto>.Success(await GetByIdAsync(category.Id, ct) ?? new CategoryDto());
    }

    public async Task<Result<CategoryDto>> UpdateAsync(UpdateCategoryDto dto, CancellationToken ct = default)
    {
        var category = await _uow.Categories.GetByIdAsync(dto.Id, ct);
        if (category == null || category.IsDeleted)
            return Result<CategoryDto>.Failure("Category not found.");

        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? SlugHelper.GenerateSlug(dto.Name)
            : dto.Slug;

        if (await _uow.Categories.SlugExistsAsync(slug, dto.Id, ct))
            return Result<CategoryDto>.Failure($"Slug '{slug}' already exists.");

        category.Name = dto.Name;
        category.Slug = slug;
        category.Description = dto.Description;
        category.ImageUrl = dto.ImageUrl;
        category.ParentId = dto.ParentId;
        category.IsActive = dto.IsActive;
        category.DisplayOrder = dto.DisplayOrder;
        category.UpdatedAt = DateTime.UtcNow;

        _uow.Categories.Update(category);
        await _uow.SaveChangesAsync(ct);

        return Result<CategoryDto>.Success(await GetByIdAsync(category.Id, ct) ?? new CategoryDto());
    }

    // ===== SOFT DELETE (đã có, refactor lại rõ hơn) =====
    public async Task<Result> SoftDeleteAsync(int id, CancellationToken ct = default)
    {
        var category = await _uow.Categories.GetByIdAsync(id, ct);
        if (category == null || category.IsDeleted)
            return Result.Failure("Category not found.");

        var hasActiveProducts = await _uow.Products
            .ExistsAsync(p => p.CategoryId == id && !p.IsDeleted, ct);
        if (hasActiveProducts)
            return Result.Failure("Cannot delete category with existing active products.");

        // Soft delete các sub-categories (nếu có)
        var subCategories = await _uow.Categories.Query()
            .Where(c => c.ParentId == id && !c.IsDeleted)
            .ToListAsync(ct);

        foreach (var sub in subCategories)
        {
            sub.IsDeleted = true;
            sub.UpdatedAt = DateTime.UtcNow;
            _uow.Categories.Update(sub);
        }

        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        _uow.Categories.Update(category);
        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }

    // ===== HARD DELETE (xóa vĩnh viễn) =====
    public async Task<Result> HardDeleteAsync(int id, CancellationToken ct = default)
    {
        var category = await _uow.Categories.Query()
            .IgnoreQueryFilters()   // bỏ global filter để truy vấn cả deleted lẫn non-deleted
            .Include(c => c.Products)
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (category == null)
            return Result.Failure("Category not found.");

        // Chặn nếu còn sản phẩm (kể cả đã soft delete)
        if (category.Products.Any())
            return Result.Failure("Cannot permanently delete category that has products. Remove all products first.");

        // Chặn nếu còn sub-categories
        if (category.Children.Any())
            return Result.Failure("Cannot permanently delete category that has sub-categories.");

        _uow.Categories.Remove(category);
        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }

    // ===== RESTORE (khôi phục soft deleted) =====
    public async Task<Result<CategoryDto>> RestoreAsync(int id, CancellationToken ct = default)
    {
        var category = await _uow.Categories.Query()
            .IgnoreQueryFilters()   // bỏ global filter để truy vấn cả deleted lẫn non-deleted
            .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted, ct);

        if (category == null)
            return Result<CategoryDto>.Failure("Deleted category not found.");

        // Kiểm tra parent có đang bị xóa không
        if (category.ParentId.HasValue)
        {
            var parentDeleted = await _uow.Categories.Query()
                .IgnoreQueryFilters()
                .AnyAsync(c => c.Id == category.ParentId && c.IsDeleted, ct);

            if (parentDeleted)
                return Result<CategoryDto>.Failure("Cannot restore: parent category is also deleted. Restore parent first.");
        }

        category.IsDeleted = false;
        category.UpdatedAt = DateTime.UtcNow;
        _uow.Categories.Update(category);
        await _uow.SaveChangesAsync(ct);

        return Result<CategoryDto>.Success(await GetByIdAsync(category.Id, ct) ?? new CategoryDto());
    }

    // ===== GET DELETED (xem danh sách đã soft delete) =====
    public async Task<PagedResult<CategoryListDto>> GetDeletedAsync(CategoryFilterParams filter, CancellationToken ct = default)
    {
        var query = _uow.Categories.Query()
            .IgnoreQueryFilters()   // bỏ global filter để truy vấn cả deleted lẫn non-deleted
            .Where(c => c.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(search) ||
                                    c.Slug.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(ct);

        query = filter.SortBy?.ToLower() switch
        {
            "name"      => filter.SortDirection == "desc" ? query.OrderByDescending(c => c.Name)      : query.OrderBy(c => c.Name),
            "updatedat" => filter.SortDirection == "desc" ? query.OrderByDescending(c => c.UpdatedAt) : query.OrderBy(c => c.UpdatedAt),
            _           => query.OrderByDescending(c => c.UpdatedAt)
        };

        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(c => new CategoryListDto
            {
                Id           = c.Id,
                Name         = c.Name,
                Slug         = c.Slug,
                IsActive     = c.IsActive,
                ParentId     = c.ParentId,
                ParentName   = c.Parent != null ? c.Parent.Name : null,
                DisplayOrder = c.DisplayOrder,
                ProductCount = c.Products.Count(),   // kể cả deleted products
                CreatedAt    = c.CreatedAt
            })
            .ToListAsync(ct);

        return PagedResult<CategoryListDto>.Create(items, totalCount, filter.PageNumber, filter.PageSize);
    }
//------------------------------------------------
    public async Task<List<CategoryListDto>> GetHomeCategories(CancellationToken ct)
    {
        return await _uow.Categories.Query()
            .AsNoTracking()
            .Where(c => c.IsActive && c.ParentId == null && !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .Take(8)
            .Select(c => new CategoryListDto
            {
                Id           = c.Id,
                Name         = c.Name,
                Slug         = c.Slug,
                IsActive     = c.IsActive,
                ParentId     = c.ParentId,
                ParentName   = c.Parent != null ? c.Parent.Name : null,
                DisplayOrder = c.DisplayOrder,
                ProductCount = c.Products.Count(),   // kể cả deleted products
                CreatedAt    = c.CreatedAt
            })
            .ToListAsync(ct);
    }

    public async Task<PagedResult<CategoryListDto>> GetBySlugAsync(string slug, CategoryFilterParams filter, CancellationToken ct = default)
    {
        var query = _uow.Categories.Query()
            .Where(c => c.Slug == slug && !c.IsDeleted)
            .AsNoTracking();

        // Search
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(search) ||
                                    c.Slug.ToLower().Contains(search));
        }

        // Filters
        if (filter.IsActive.HasValue)
            query = query.Where(c => c.IsActive == filter.IsActive.Value);

        if (filter.ParentId.HasValue)
            query = query.Where(c => c.ParentId == filter.ParentId.Value);

        // Total count (performance: count before projection)
        var totalCount = await query.CountAsync(ct);

        // Sorting
        query = filter.SortBy?.ToLower() switch
        {
            "name" => filter.SortDirection == "desc" ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "displayorder" => filter.SortDirection == "desc" ? query.OrderByDescending(c => c.DisplayOrder) : query.OrderBy(c => c.DisplayOrder),
            "createdat" => filter.SortDirection == "desc" ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => query.OrderBy(c => c.Name)
        };

        // Pagination + Projection (single query, no N+1)
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(c => new CategoryListDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                IsActive = c.IsActive,
                ParentId = c.ParentId,
                ParentName = c.Parent != null ? c.Parent.Name : null,
                DisplayOrder = c.DisplayOrder,
                ProductCount = c.Products.Count(p => !p.IsDeleted),
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(ct);

        return PagedResult<CategoryListDto>.Create(items, totalCount, filter.PageNumber, filter.PageSize);
    }

}