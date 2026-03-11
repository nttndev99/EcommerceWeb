using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Product;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Services
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly IUnitOfWork _uow;
        public ProductVariantService(IUnitOfWork uow) => _uow = uow;

        public async Task<PagedResult<ProductVariantDto>> GetPagedByProductAsync(ProductVariantFilterParams filter, CancellationToken ct = default)
        {
            var query = _uow.ProductVariants.Query()
                .Where(v => v.ProductId == filter.ProductId && !v.IsDeleted)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.ToLower();
                query = query.Where(v => v.Name.ToLower().Contains(s) || (v.SKU != null && v.SKU.ToLower().Contains(s)));
            }

            if (filter.IsActive.HasValue)
                query = query.Where(v => v.IsActive == filter.IsActive.Value);

            var totalCount = await query.CountAsync(ct);

            query = filter.SortBy?.ToLower() switch
            {
                "name" => filter.SortDirection == "desc" ? query.OrderByDescending(v => v.Name) : query.OrderBy(v => v.Name),
                "price" => filter.SortDirection == "desc" ? query.OrderByDescending(v => v.Price) : query.OrderBy(v => v.Price),
                _ => query.OrderBy(v => v.DisplayOrder)
            };

            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(v => new ProductVariantDto
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
                })
                .ToListAsync(ct);

            return PagedResult<ProductVariantDto>.Create(items, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<ProductVariantDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _uow.ProductVariants.Query()
                .Where(v => v.Id == id && !v.IsDeleted)
                .AsNoTracking()
                .Select(v => new ProductVariantDto
                {
                    Id = v.Id, ProductId = v.ProductId, Name = v.Name, SKU = v.SKU,
                    Price = v.Price, SalePrice = v.SalePrice, Color = v.Color, Size = v.Size,
                    Material = v.Material, ImageUrl = v.ImageUrl, IsActive = v.IsActive,
                    DisplayOrder = v.DisplayOrder,
                    Stock = v.Inventories.Sum(i => i.Quantity - i.ReservedQuantity)
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<Result<ProductVariantDto>> CreateAsync(CreateProductVariantDto dto, CancellationToken ct = default)
        {
            if (!string.IsNullOrWhiteSpace(dto.SKU) && await _uow.ProductVariants.SKUExistsAsync(dto.SKU, null, ct))
                return Result<ProductVariantDto>.Failure($"SKU '{dto.SKU}' already exists.");

            var variant = new ProductVariant
            {
                ProductId = dto.ProductId,
                Name = dto.Name,
                SKU = dto.SKU,
                Price = dto.Price,
                SalePrice = dto.SalePrice,
                Color = dto.Color,
                Size = dto.Size,
                Material = dto.Material,
                ImageUrl = dto.ImageUrl,
                IsActive = dto.IsActive,
                DisplayOrder = dto.DisplayOrder
            };
            await _uow.ProductVariants.AddAsync(variant, ct);
            await _uow.SaveChangesAsync(ct);

            // Create inventory for variant
            await _uow.Inventories.AddAsync(new Inventory
            {
                ProductId = dto.ProductId,
                ProductVariantId = variant.Id,
                Quantity = dto.InitialStock
            }, ct);
            await _uow.SaveChangesAsync(ct);

            return Result<ProductVariantDto>.Success(await GetByIdAsync(variant.Id, ct) ?? new ProductVariantDto());
        }

        public async Task<Result<ProductVariantDto>> UpdateAsync(UpdateProductVariantDto dto, CancellationToken ct = default)
        {
            var variant = await _uow.ProductVariants.GetByIdAsync(dto.Id, ct);
            if (variant == null || variant.IsDeleted)
                return Result<ProductVariantDto>.Failure("Variant not found.");

            if (!string.IsNullOrWhiteSpace(dto.SKU) && await _uow.ProductVariants.SKUExistsAsync(dto.SKU, dto.Id, ct))
                return Result<ProductVariantDto>.Failure($"SKU '{dto.SKU}' already exists.");

            variant.Name = dto.Name;
            variant.SKU = dto.SKU;
            variant.Price = dto.Price;
            variant.SalePrice = dto.SalePrice;
            variant.Color = dto.Color;
            variant.Size = dto.Size;
            variant.Material = dto.Material;
            variant.ImageUrl = dto.ImageUrl;
            variant.IsActive = dto.IsActive;
            variant.DisplayOrder = dto.DisplayOrder;
            variant.UpdatedAt = DateTime.UtcNow;

            _uow.ProductVariants.Update(variant);
            await _uow.SaveChangesAsync(ct);
            return Result<ProductVariantDto>.Success(await GetByIdAsync(variant.Id, ct) ?? new ProductVariantDto());
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
        {
            var variant = await _uow.ProductVariants.GetByIdAsync(id, ct);
            if (variant == null || variant.IsDeleted)
                return Result.Failure("Variant not found.");
            variant.IsDeleted = true;
            variant.UpdatedAt = DateTime.UtcNow;
            _uow.ProductVariants.Update(variant);
            await _uow.SaveChangesAsync(ct);
            return Result.Success();
        }
    }

}