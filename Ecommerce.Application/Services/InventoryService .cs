using Microsoft.EntityFrameworkCore;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Inventory;
using Ecommerce.Application.DTOs.Product;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IUnitOfWork _uow;

    public InventoryService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    // ─────────────────────────────────────────────
    // GET PAGED
    // ─────────────────────────────────────────────
    public async Task<PagedResult<InventoryDto>> GetPagedAsync(
        InventoryFilterParams filter, CancellationToken ct = default)
    {
        var query = _uow.Inventories.Query()
            .Include(i => i.Product).ThenInclude(p => p.Category)
            .Include(i => i.ProductVariant)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var kw = filter.Search.Trim().ToLower();
            query = query.Where(i =>
                i.Product.Name.ToLower().Contains(kw) ||
                (i.WarehouseLocation != null && i.WarehouseLocation.ToLower().Contains(kw)));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(i => i.Product.CategoryId == filter.CategoryId.Value);

        if (filter.IsLowStock == true)
            query = query.Where(i => (i.Quantity - i.ReservedQuantity) <= i.LowStockThreshold);

        if (filter.IsOutOfStock == true)
            query = query.Where(i => (i.Quantity - i.ReservedQuantity) <= 0);

        query = (filter.SortBy?.ToLower(), filter.SortDirection?.ToLower()) switch
        {
            ("quantity",   "desc") => query.OrderByDescending(i => i.Quantity),
            ("quantity",   _)      => query.OrderBy(i => i.Quantity),
            ("available",  "desc") => query.OrderByDescending(i => i.Quantity - i.ReservedQuantity),
            ("available",  _)      => query.OrderBy(i => i.Quantity - i.ReservedQuantity),
            ("lastupdate", "desc") => query.OrderByDescending(i => i.LastStockUpdate),
            ("lastupdate", _)      => query.OrderBy(i => i.LastStockUpdate),
            (_,            "desc") => query.OrderByDescending(i => i.Product.Name),
            _                      => query.OrderBy(i => i.Product.Name),
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(i => MapToDto(i))
            .ToListAsync(ct);

        return PagedResult<InventoryDto>.Create(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    // ─────────────────────────────────────────────
    // GET BY ID
    // ─────────────────────────────────────────────
    public async Task<InventoryDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var inv = await _uow.Inventories.Query()
            .Include(i => i.Product)
            .Include(i => i.ProductVariant)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, ct);

        return inv is null ? null : MapToDto(inv);
    }

    // ─────────────────────────────────────────────
    // GET BY PRODUCT ID
    // ─────────────────────────────────────────────
    public async Task<ProductDto?> GetByProductIdAsync(int productId, CancellationToken ct = default)
    {
        var product = await _uow.Products.Query()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Inventories).ThenInclude(i => i.ProductVariant)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted, ct);

        if (product is null) return null;

        return new ProductDto
        {
            Id               = product.Id,
            Name             = product.Name,
            Slug             = product.Slug,
            Description      = product.Description,
            ShortDescription = product.ShortDescription,
            BasePrice        = product.BasePrice,
            SalePrice        = product.SalePrice,
            SKU              = product.SKU,
            Status           = product.Status,
            IsFeatured       = product.IsFeatured,
            CategoryId       = product.CategoryId,
            CategoryName     = product.Category?.Name ?? string.Empty,
            Brand            = product.Brand,
            Tags             = product.Tags,
            PrimaryImageUrl  = product.Images?
                .OrderBy(img => img.DisplayOrder)
                .FirstOrDefault(img => img.IsPrimary)?.ImageUrl,
            TotalStock   = product.Inventories?.Sum(i => i.AvailableQuantity) ?? 0,
            VariantCount = product.Variants?.Count ?? 0,
            CreatedAt    = product.CreatedAt,
            Inventories  = product.Inventories?.Select(i => MapToDto(i)).ToList() ?? new(),
            Images       = product.Images?
                .OrderBy(img => img.DisplayOrder)
                .Select(img => new ProductImageDto
                {
                    Id           = img.Id,
                    ImageUrl     = img.ImageUrl,
                    AltText      = img.AltText,
                    IsPrimary    = img.IsPrimary,
                    DisplayOrder = img.DisplayOrder,
                }).ToList() ?? new(),
        };
    }

    // ─────────────────────────────────────────────
    // UPDATE STOCK  →  log "update"
    // ─────────────────────────────────────────────
    public async Task<Result<InventoryDto>> UpdateStockAsync(
        UpdateInventoryDto dto, CancellationToken ct = default)
    {
        var inv = await _uow.Inventories.Query()
            .Include(i => i.Product)
            .Include(i => i.ProductVariant)
            .FirstOrDefaultAsync(i => i.Id == dto.Id, ct);

        if (inv is null)
            return Result<InventoryDto>.Failure($"Inventory record #{dto.Id} not found.");
        if (dto.Quantity < 0)
            return Result<InventoryDto>.Failure("Quantity cannot be negative.");
        if (dto.ReservedQuantity < 0)
            return Result<InventoryDto>.Failure("Reserved quantity cannot be negative.");
        if (dto.ReservedQuantity > dto.Quantity)
            return Result<InventoryDto>.Failure("Reserved quantity cannot exceed total quantity.");

        int before = inv.Quantity;

        inv.Quantity          = dto.Quantity;
        inv.ReservedQuantity  = dto.ReservedQuantity;
        inv.LowStockThreshold = dto.LowStockThreshold;
        inv.WarehouseLocation = dto.WarehouseLocation;
        inv.LastStockUpdate   = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        return Result<InventoryDto>.Success(MapToDto(inv));
    }

    // ─────────────────────────────────────────────
    // ADJUST  →  log "add" | "subtract" | "set"
    // ─────────────────────────────────────────────
    public async Task<Result<InventoryDto>> AdjustAsynckAsync(
        AdjustInventoryDto dto, CancellationToken ct = default)
    {
        var inv = await _uow.Inventories.Query()
            .Include(i => i.Product)
            .Include(i => i.ProductVariant)
            .FirstOrDefaultAsync(i => i.Id == dto.Id, ct);

        if (inv is null)
            return Result<InventoryDto>.Failure($"Inventory record #{dto.Id} not found.");

        int before = inv.Quantity;

        switch (dto.AdjustmentType.ToLower())
        {
            case "add":
                if (dto.AdjustmentQuantity <= 0)
                    return Result<InventoryDto>.Failure("Add quantity must be greater than 0.");
                inv.Quantity += dto.AdjustmentQuantity;
                break;

            case "subtract":
                if (dto.AdjustmentQuantity <= 0)
                    return Result<InventoryDto>.Failure("Subtract quantity must be greater than 0.");
                if (inv.AvailableQuantity < dto.AdjustmentQuantity)
                    return Result<InventoryDto>.Failure(
                        $"Insufficient stock. Available: {inv.AvailableQuantity}, requested: {dto.AdjustmentQuantity}.");
                inv.Quantity -= dto.AdjustmentQuantity;
                break;

            case "set":
                if (dto.AdjustmentQuantity < 0)
                    return Result<InventoryDto>.Failure("Set quantity cannot be negative.");
                inv.Quantity = dto.AdjustmentQuantity;
                break;

            default:
                return Result<InventoryDto>.Failure(
                    "Invalid AdjustmentType. Use: 'add', 'subtract', or 'set'.");
        }

        inv.LastStockUpdate = DateTime.UtcNow;
        await _uow.SaveChangesAsync(ct);


        return Result<InventoryDto>.Success(MapToDto(inv));
    }

    // ─────────────────────────────────────────────
    // LOW STOCK
    // ─────────────────────────────────────────────
    public async Task<IEnumerable<InventoryDto>> GetLowStockItemsAsync(CancellationToken ct = default)
    {
        var items = await _uow.Inventories.Query()
            .Include(i => i.Product)
            .Include(i => i.ProductVariant)
            .AsNoTracking()
            .Where(i => (i.Quantity - i.ReservedQuantity) <= i.LowStockThreshold)
            .OrderBy(i => i.Quantity - i.ReservedQuantity)
            .ToListAsync(ct);

        return items.Select(MapToDto);
    }

    // ─────────────────────────────────────────────
    // COUNT
    // ─────────────────────────────────────────────
    public async Task<int> CountAsync(InventoryFilterParams filter, CancellationToken ct = default)
    {
        var query = _uow.Inventories.Query()
            .Include(i => i.Product)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var kw = filter.Search.Trim().ToLower();
            query = query.Where(i => i.Product.Name.ToLower().Contains(kw));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(i => i.Product.CategoryId == filter.CategoryId.Value);

        if (filter.IsLowStock == true)
            query = query.Where(i => (i.Quantity - i.ReservedQuantity) <= i.LowStockThreshold);

        if (filter.IsOutOfStock == true)
            query = query.Where(i => (i.Quantity - i.ReservedQuantity) <= 0);

        return await query.CountAsync(ct);
    }

    // ─────────────────────────────────────────────
    // MAPPER
    // ─────────────────────────────────────────────
    private static InventoryDto MapToDto(Inventory i) => new()
    {
        Id                = i.Id,
        ProductId         = i.ProductId,
        ProductName       = i.Product?.Name ?? string.Empty,
        ProductVariantId  = i.ProductVariantId,
        VariantName       = i.ProductVariant?.Name,
        Quantity          = i.Quantity,
        ReservedQuantity  = i.ReservedQuantity,
        AvailableQuantity = i.AvailableQuantity,
        LowStockThreshold = i.LowStockThreshold,
        WarehouseLocation = i.WarehouseLocation,
        IsLowStock        = i.IsLowStock,
        IsOutOfStock      = i.IsOutOfStock,
        LastStockUpdate   = i.LastStockUpdate,
    };
}