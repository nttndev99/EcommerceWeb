using System.Linq.Expressions;
using System.Text.Json;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Inventory;
using Ecommerce.Application.DTOs.Product;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _uow;

    public ProductService(IUnitOfWork uow) => _uow = uow;

    // ────────────────────────────────────────────────────────────────────────────
    //  GET PAGED
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PagedResult<ProductListDto>> GetPagedAsync(ProductFilterParams filter, CancellationToken ct = default)
    {
        var pageSize   = filter.PageSize   > 0 ? filter.PageSize   : 10;
        var pageNumber = filter.PageNumber > 0 ? filter.PageNumber : 1;

        // 1. Base query qua Repository — không dùng DbContext trực tiếp
        IQueryable<Product> query = _uow.Products.Query().Where(p => !p.IsDeleted).AsNoTracking();

        // 2. Apply filters
        query = ApplyFilters(query, filter);

        // 3. Count trước khi sort / page
        var totalCount = await query.CountAsync(ct);

        // 4. Sort
        IOrderedQueryable<Product> sorted = ApplySort(query, filter);

        // 5. Page + Project
        var items = await sorted
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                BasePrice = p.BasePrice,
                SalePrice = p.SalePrice,
                SKU = p.SKU,
                Status = p.Status,
                IsFeatured = p.IsFeatured,
                Brand = p.Brand,
                CreatedAt = p.CreatedAt,
                VariantCount = _uow.ProductVariants.Query()
                    .Where(v => v.ProductId == p.Id && !v.IsDeleted)
                    .Count(),
                CategoryName = _uow.Categories.Query()
                    .Where(c => c.Id == p.CategoryId)
                    .Select(c => c.Name)
                    .FirstOrDefault(), // NOTE: tranh NULL khi category bi xoa hoac khong co => tranh loi paginate

                TotalStock = _uow.Inventories.Query()
                    .Where(i => i.ProductId == p.Id)
                    .Sum(i => (int?)(i.Quantity - i.ReservedQuantity)) ?? 0, // NOTE: tranh null khi product chua co inventory nao => tranh loi paginate

                PrimaryImageUrl = p.Images
                    .Where(i => i.IsPrimary)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        return PagedResult<ProductListDto>.Create(items, totalCount, pageNumber, pageSize);
    }

    // ────────────────────────────────────────────────────────────────────────────
    //  GET BY ID
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var dto = await _uow.Products.Query()
            .Where(p => p.Id == id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                BasePrice = p.BasePrice,
                SalePrice = p.SalePrice,
                Description      = p.Description,
                ShortDescription = p.ShortDescription,
                SKU = p.SKU,
                Status = p.Status,
                IsFeatured = p.IsFeatured,
                Brand = p.Brand,
                CreatedAt = p.CreatedAt,
                CategoryName = _uow.Categories.Query()
                    .Where(c => c.Id == p.CategoryId)
                    .Select(c => c.Name)
                    .FirstOrDefault(), // NOTE: tranh NULL khi category bi xoa hoac khong co => tranh loi paginate

                TotalStock = _uow.Inventories.Query()
                    .Where(i => i.ProductId == p.Id)
                    .Sum(i => (int?)(i.Quantity - i.ReservedQuantity)) ?? 0, // NOTE: tranh null khi product chua co inventory nao => tranh loi paginate

                PrimaryImageUrl = p.Images
                    .Where(i => i.IsPrimary)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        return dto;

    }

    // ────────────────────────────────────────────────────────────────────────────
    //  CREATE
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<Result<ProductDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? SlugHelper.GenerateSlug(dto.Name)
            : dto.Slug;

        if (await _uow.Products.SlugExistsAsync(slug, null, ct))
            return Result<ProductDto>.Failure($"Slug '{slug}' already exists.");

        if (!await _uow.Categories.ExistsAsync(c => c.Id == dto.CategoryId && !c.IsDeleted, ct))
            return Result<ProductDto>.Failure("Category not found.");

        var product = new Product
        {
            Name             = dto.Name,
            Slug             = slug,
            Description      = dto.Description,
            ShortDescription = dto.ShortDescription,
            BasePrice        = dto.BasePrice,
            SalePrice        = dto.SalePrice,
            SKU              = dto.SKU,
            Status           = dto.Status,
            IsFeatured       = dto.IsFeatured,
            CategoryId       = dto.CategoryId,
            Brand            = dto.Brand,
            Weight           = dto.Weight,
            Tags             = dto.Tags
        };

        await _uow.Products.AddAsync(product, ct);
        await _uow.SaveChangesAsync(ct);

        // Default inventory (product-level, chưa có variant)
        await _uow.Inventories.AddAsync(new Inventory
        {
            ProductId        = product.Id,
            ProductVariantId = null,
            Quantity         = 0,
            ReservedQuantity = 0
        }, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<ProductDto>.Success(await GetByIdAsync(product.Id, ct) ?? new ProductDto());
    }

    // ────────────────────────────────────────────────────────────────────────────
    //  UPDATE
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<Result<ProductDto>> UpdateAsync(
        UpdateProductDto dto, CancellationToken ct = default)
    {
        var product = await _uow.Products.GetByIdAsync(dto.Id, ct);
        if (product is null || product.IsDeleted)
            return Result<ProductDto>.Failure("Product not found.");

        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? SlugHelper.GenerateSlug(dto.Name)
            : dto.Slug;

        if (await _uow.Products.SlugExistsAsync(slug, dto.Id, ct))
            return Result<ProductDto>.Failure($"Slug '{slug}' already exists.");

        product.Name             = dto.Name;
        product.Slug             = slug;
        product.Description      = dto.Description;
        product.ShortDescription = dto.ShortDescription;
        product.BasePrice        = dto.BasePrice;
        product.SalePrice        = dto.SalePrice;
        product.SKU              = dto.SKU;
        product.Status           = dto.Status;
        product.IsFeatured       = dto.IsFeatured;
        product.CategoryId       = dto.CategoryId;
        product.Brand            = dto.Brand;
        product.Tags             = dto.Tags;
        product.UpdatedAt        = DateTime.UtcNow;

        _uow.Products.Update(product);
        await _uow.SaveChangesAsync(ct);

        return Result<ProductDto>.Success(await GetByIdAsync(product.Id, ct) ?? new ProductDto());
    }

    // ────────────────────────────────────────────────────────────────────────────
    //  SOFT DELETE
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<Result> SoftDeleteAsync(int id, CancellationToken ct = default)
    {
        var product = await _uow.Products.GetByIdAsync(id, ct);

        if (product == null || product.IsDeleted)
            return Result.Failure("Product not found.");

        // Soft delete variants
        var variants = await _uow.ProductVariants.Query()
            .Where(v => v.ProductId == id && !v.IsDeleted)
            .ToListAsync(ct);

        foreach (var variant in variants)
        {
            variant.IsDeleted = true;
            variant.UpdatedAt = DateTime.UtcNow;
            _uow.ProductVariants.Update(variant);
        }

        // Soft delete product
        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;

        _uow.Products.Update(product);

        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }

    // ────────────────────────────────────────────────────────────────────────────
    //  HARD DELETE
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<Result> HardDeleteAsync(int id, CancellationToken ct = default)
    {
        var product = await _uow.Products.Query()
            .IgnoreQueryFilters()
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product is null)
            return Result.Failure("Product not found.");

        if (product.Variants.Any())
            return Result.Failure("Cannot permanently delete a product that still has variants.");

        _uow.Products.Remove(product);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    // ────────────────────────────────────────────────────────────────────────────
    //  RESTORE
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<Result<ProductDto>> RestoreAsync(int id, CancellationToken ct = default)
    {
        var product = await _uow.Products.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted, ct);

        if (product is null)
            return Result<ProductDto>.Failure("Deleted product not found.");

        product.IsDeleted = false;
        product.UpdatedAt = DateTime.UtcNow;
        _uow.Products.Update(product);
        await _uow.SaveChangesAsync(ct);

        return Result<ProductDto>.Success(await GetByIdAsync(product.Id, ct) ?? new ProductDto());
    }

    // ────────────────────────────────────────────────────────────────────────────
    //  GET DELETED
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PagedResult<ProductListDto>> GetDeletedAsync(ProductFilterParams filter, CancellationToken ct = default)
    {
        var pageSize   = filter.PageSize   > 0 ? filter.PageSize   : 10;
        var pageNumber = filter.PageNumber > 0 ? filter.PageNumber : 1;

        // 1. Base query qua Repository — không dùng DbContext trực tiếp
        IQueryable<Product> query = _uow.Products.Query().IgnoreQueryFilters().Where(p => p.IsDeleted == true).AsNoTracking();

        // 2. Apply filters
        query = ApplyFilters(query, filter);

        // 3. Count trước khi sort / page
        var totalCount = await query.CountAsync(ct);

        // 4. Sort
        IOrderedQueryable<Product> sorted = ApplySort(query, filter);

        // 5. Page + Project
        var items = await sorted
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                BasePrice = p.BasePrice,
                SalePrice = p.SalePrice,
                SKU = p.SKU,
                Status = p.Status,
                IsFeatured = p.IsFeatured,
                Brand = p.Brand,
                CreatedAt = p.CreatedAt,
                CategoryName = p.Category != null ? p.Category.Name : null, // NOTE: tranh NULL khi category bi xoa hoac khong co => tranh loi paginate

                TotalStock = _uow.Inventories.Query()
                    .Where(i => i.ProductId == p.Id)
                    .Sum(i => (int?)(i.Quantity - i.ReservedQuantity)) ?? 0, // NOTE: tranh null khi product chua co inventory nao => tranh loi paginate

                PrimaryImageUrl = p.Images
                    .Where(i => i.IsPrimary)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        return PagedResult<ProductListDto>.Create(items, totalCount, pageNumber, pageSize);
    }

    // ────────────────────────────────────────────────────────────────────────────
    //  PRIVATE HELPERS
    // ────────────────────────────────────────────────────────────────────────────

    private static IQueryable<Product> ApplyFilters(IQueryable<Product> query, ProductFilterParams f)
    {
        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var s = f.Search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(s) ||
                (p.SKU  != null && p.SKU.ToLower().Contains(s)) ||
                (p.Brand != null && p.Brand.ToLower().Contains(s)));
        }

        if (f.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == f.CategoryId.Value);

        if (f.Status.HasValue)
            query = query.Where(p => p.Status == f.Status.Value);

        if (f.IsFeatured.HasValue)
            query = query.Where(p => p.IsFeatured == f.IsFeatured.Value);

        if (!string.IsNullOrWhiteSpace(f.Brand))
            query = query.Where(p =>
                p.Brand != null && p.Brand.ToLower().Contains(f.Brand.ToLower()));

        if (f.MinPrice.HasValue)
            query = query.Where(p => p.BasePrice >= f.MinPrice.Value);

        if (f.MaxPrice.HasValue)
            query = query.Where(p => p.BasePrice <= f.MaxPrice.Value);

        if (f.InStock.HasValue)
            query = f.InStock.Value
                ? query.Where(p => p.Inventories.Any(i => i.Quantity - i.ReservedQuantity > 0))
                : query.Where(p => !p.Inventories.Any(i => i.Quantity - i.ReservedQuantity > 0));

        return query;
    }

    private static IOrderedQueryable<Product> ApplySort( IQueryable<Product> query, ProductFilterParams f)
    {
        var desc = f.SortDirection?.ToLower() == "desc";

        return f.SortBy?.ToLower() switch
        {
            "name"   => desc ? query.OrderByDescending(p => p.Name)      : query.OrderBy(p => p.Name),
            "price"  => desc ? query.OrderByDescending(p => p.BasePrice) : query.OrderBy(p => p.BasePrice),
            "status" => desc ? query.OrderByDescending(p => p.Status)    : query.OrderBy(p => p.Status),
            _        => desc ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
        };
    }


//-------------------------
    public async Task<List<ProductListDto>> GetHomeProducts(string tab, CancellationToken ct)
    {
        var query = _uow.Products.Query()
            .AsNoTracking()
            .Where(p => p.Status == ProductStatus.Active && !p.IsDeleted)

            // KHONG LAY OUT OF STOCK
            .Where(p => _uow.Inventories.Query()
                .Where(i => i.ProductId == p.Id)
                .Sum(i => (int?)(i.Quantity - i.ReservedQuantity)) > 0);

        switch (tab)
        {
            case "new":
                query = query
                    .OrderByDescending(p => p.CreatedAt);
                break;

            case "sale":
                query = query
                    .Where(p => p.SalePrice != null)
                    .OrderByDescending(p => p.CreatedAt);
                break;

            default: // best
                query = query
                    .Where(p => p.IsFeatured)
                    .OrderByDescending(p => p.CreatedAt);
                break;
        }

        return await query
            .Take(8)
            .Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                BasePrice = p.BasePrice,
                SalePrice = p.SalePrice,
                SKU = p.SKU,
                Status = p.Status,
                IsFeatured = p.IsFeatured,
                Brand = p.Brand,
                CreatedAt = p.CreatedAt,

                CategoryName = p.Category != null ? p.Category.Name : null,

                TotalStock = _uow.Inventories.Query()
                    .Where(i => i.ProductId == p.Id)
                    .Sum(i => (int?)(i.Quantity - i.ReservedQuantity)) ?? 0,

                PrimaryImageUrl = p.Images
                    .Where(i => i.IsPrimary)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault(),

                IsNew = p.CreatedAt > DateTime.UtcNow.AddDays(-7)
            })
            .ToListAsync(ct);
    }



    public async Task<PagedResult<ProductListDto>> GetBySlugAsync(string slug,ProductFilterParams filter, CancellationToken ct = default)
    {
        var pageSize   = filter.PageSize   > 0 ? filter.PageSize   : 10;
        var pageNumber = filter.PageNumber > 0 ? filter.PageNumber : 1;

        // 1. Base query qua Repository — không dùng DbContext trực tiếp
        IQueryable<Product> query = _uow.Products.Query()
        .Where(p => p.Slug == slug && !p.IsDeleted).AsNoTracking();

        // 2. Apply filters
        query = ApplyFilters(query, filter);

        // 3. Count trước khi sort / page
        var totalCount = await query.CountAsync(ct);

        // 4. Sort
        IOrderedQueryable<Product> sorted = ApplySort(query, filter);

        // 5. Page + Project
        var items = await sorted
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                BasePrice = p.BasePrice,
                SalePrice = p.SalePrice,
                SKU = p.SKU,
                Status = p.Status,
                IsFeatured = p.IsFeatured,
                Brand = p.Brand,
                CreatedAt = p.CreatedAt,
                VariantCount = _uow.ProductVariants.Query()
                    .Where(v => v.ProductId == p.Id && !v.IsDeleted)
                    .Count(),
                CategoryName = _uow.Categories.Query()
                    .Where(c => c.Id == p.CategoryId)
                    .Select(c => c.Name)
                    .FirstOrDefault(), // NOTE: tranh NULL khi category bi xoa hoac khong co => tranh loi paginate

                TotalStock = _uow.Inventories.Query()
                    .Where(i => i.ProductId == p.Id)
                    .Sum(i => (int?)(i.Quantity - i.ReservedQuantity)) ?? 0, // NOTE: tranh null khi product chua co inventory nao => tranh loi paginate

                PrimaryImageUrl = p.Images
                    .Where(i => i.IsPrimary)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        return PagedResult<ProductListDto>.Create(items, totalCount, pageNumber, pageSize);
    }















}







