
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Inventory;
using Ecommerce.Application.DTOs.Product;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _uow;

    public ProductService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<PagedResult<ProductListDto>> GetPagedAsync(ProductFilterParams filter, CancellationToken ct = default)
    {
        var query = _uow.Products.Query()
            .Where(p => !p.IsDeleted)
            .AsNoTracking();

        // Search
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(search) ||
                p.SKU!.ToLower().Contains(search) ||
                p.Brand!.ToLower().Contains(search));
        }

        // Filters
        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

        if (filter.Status.HasValue)
            query = query.Where(p => p.Status == filter.Status.Value);

        if (filter.IsFeatured.HasValue)
            query = query.Where(p => p.IsFeatured == filter.IsFeatured.Value);

        if (!string.IsNullOrWhiteSpace(filter.Brand))
            query = query.Where(p => p.Brand != null && p.Brand.ToLower().Contains(filter.Brand.ToLower()));

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.BasePrice >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.BasePrice <= filter.MaxPrice.Value);

        if (filter.InStock.HasValue)
        {
            query = filter.InStock.Value
                ? query.Where(p => p.Inventories.Any(i => i.Quantity - i.ReservedQuantity > 0))
                : query.Where(p => !p.Inventories.Any(i => i.Quantity - i.ReservedQuantity > 0));
        }

        var totalCount = await query.CountAsync(ct);

        // Sorting
        query = filter.SortBy?.ToLower() switch
        {
            "name" => filter.SortDirection == "desc" ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => filter.SortDirection == "desc" ? query.OrderByDescending(p => p.BasePrice) : query.OrderBy(p => p.BasePrice),
            "status" => filter.SortDirection == "desc" ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status),
            _ => filter.SortDirection == "desc" ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
        };

        // Optimized projection
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
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
                CategoryName = p.Category.Name,
                Brand = p.Brand,
                PrimaryImageUrl = p.Images
                    .Where(i => i.IsPrimary)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault(),
                TotalStock = p.Inventories.Sum(i => i.Quantity - i.ReservedQuantity),
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(ct);

        return PagedResult<ProductListDto>.Create(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _uow.Products.Query()
            .Where(p => p.Id == id && !p.IsDeleted)
            .AsNoTracking()
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Description = p.Description,
                ShortDescription = p.ShortDescription,
                BasePrice = p.BasePrice,
                SalePrice = p.SalePrice,
                SKU = p.SKU,
                Status = p.Status,
                IsFeatured = p.IsFeatured,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                Brand = p.Brand,
                Tags = p.Tags,
                PrimaryImageUrl = p.Images.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault(),
                TotalStock = p.Inventories.Sum(i => i.Quantity - i.ReservedQuantity),
                VariantCount = p.Variants.Count(v => !v.IsDeleted),
                CreatedAt = p.CreatedAt,
                Variants = p.Variants.Where(v => !v.IsDeleted).Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    ProductId = v.ProductId,
                    Name = v.Name,
                    SKU = v.SKU,
                    Price = v.Price,
                    SalePrice = v.SalePrice,
                    Color = v.Color,
                    Size = v.Size,
                    Material = v.Material,
                    ImageUrl = v.ImageUrl,
                    IsActive = v.IsActive,
                    DisplayOrder = v.DisplayOrder,
                    Stock = v.Inventories.Sum(i => i.Quantity - i.ReservedQuantity)
                }).OrderBy(v => v.DisplayOrder).ToList(),
                Images = p.Images.Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ImageUrl = i.ImageUrl,
                    AltText = i.AltText,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder
                }).OrderBy(i => i.DisplayOrder).ToList(),
                Inventories = p.Inventories.Select(i => new InventoryDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductVariantId = i.ProductVariantId,
                    VariantName = i.ProductVariant != null ? i.ProductVariant.Name : null,
                    Quantity = i.Quantity,
                    ReservedQuantity = i.ReservedQuantity,
                    AvailableQuantity = i.Quantity - i.ReservedQuantity,
                    LowStockThreshold = i.LowStockThreshold,
                    WarehouseLocation = i.WarehouseLocation,
                    IsLowStock = (i.Quantity - i.ReservedQuantity) <= i.LowStockThreshold,
                    IsOutOfStock = (i.Quantity - i.ReservedQuantity) <= 0,
                    LastStockUpdate = i.LastStockUpdate
                }).ToList()
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Result<ProductDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? SlugHelper.GenerateSlug(dto.Name)
            : dto.Slug;

        if (await _uow.Products.SlugExistsAsync(slug, null, ct))
            return Result<ProductDto>.Failure($"Slug '{slug}' already exists.");

        var categoryExists = await _uow.Categories.ExistsAsync(c => c.Id == dto.CategoryId && !c.IsDeleted, ct);
        if (!categoryExists)
            return Result<ProductDto>.Failure("Category not found.");

        var product = new Product
        {
            Name = dto.Name,
            Slug = slug,
            Description = dto.Description,
            ShortDescription = dto.ShortDescription,
            BasePrice = dto.BasePrice,
            SalePrice = dto.SalePrice,
            SKU = dto.SKU,
            Status = dto.Status,
            IsFeatured = dto.IsFeatured,
            CategoryId = dto.CategoryId,
            Brand = dto.Brand,
            Weight = dto.Weight,
            Tags = dto.Tags
        };

        await _uow.Products.AddAsync(product, ct);
        await _uow.SaveChangesAsync(ct);

        // Create default inventory record
        var inventory = new Inventory
        {
            ProductId = product.Id,
            ProductVariantId = null,
            Quantity = 0,
            ReservedQuantity = 0
        };
        await _uow.Inventories.AddAsync(inventory, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<ProductDto>.Success(await GetByIdAsync(product.Id, ct) ?? new ProductDto());
    }

    public async Task<Result<ProductDto>> UpdateAsync(UpdateProductDto dto, CancellationToken ct = default)
    {
        var product = await _uow.Products.GetByIdAsync(dto.Id, ct);
        if (product == null || product.IsDeleted)
            return Result<ProductDto>.Failure("Product not found.");

        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? SlugHelper.GenerateSlug(dto.Name)
            : dto.Slug;

        if (await _uow.Products.SlugExistsAsync(slug, dto.Id, ct))
            return Result<ProductDto>.Failure($"Slug '{slug}' already exists.");

        product.Name = dto.Name;
        product.Slug = slug;
        product.Description = dto.Description;
        product.ShortDescription = dto.ShortDescription;
        product.BasePrice = dto.BasePrice;
        product.SalePrice = dto.SalePrice;
        product.SKU = dto.SKU;
        product.Status = dto.Status;
        product.IsFeatured = dto.IsFeatured;
        product.CategoryId = dto.CategoryId;
        product.Brand = dto.Brand;
        product.Weight = dto.Weight;
        product.Tags = dto.Tags;
        product.UpdatedAt = DateTime.UtcNow;

        _uow.Products.Update(product);
        await _uow.SaveChangesAsync(ct);

        return Result<ProductDto>.Success(await GetByIdAsync(product.Id, ct) ?? new ProductDto());
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var product = await _uow.Products.GetByIdAsync(id, ct);
        if (product == null || product.IsDeleted)
            return Result.Failure("Product not found.");

        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;
        _uow.Products.Update(product);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
