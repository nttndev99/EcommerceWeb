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
    public class ProductImageService : IProductImageService
    {
        private readonly IUnitOfWork _uow;
        public ProductImageService(IUnitOfWork uow) => _uow = uow;

        public async Task<IEnumerable<ProductImageDto>> GetByProductIdAsync(int productId, CancellationToken ct = default)
        {
            return await _uow.ProductImages.Query()
                .Where(i => i.ProductId == productId && !i.IsDeleted)
                .OrderBy(i => i.DisplayOrder)
                .AsNoTracking()
                .Select(i => new ProductImageDto
                {
                    Id = i.Id, ProductId = i.ProductId, ImageUrl = i.ImageUrl,
                    AltText = i.AltText, IsPrimary = i.IsPrimary, DisplayOrder = i.DisplayOrder
                })
                .ToListAsync(ct);
        }

        public async Task<Result<ProductImageDto>> CreateAsync(CreateProductImageDto dto, CancellationToken ct = default)
        {
            // If setting as primary, unset others
            if (dto.IsPrimary)
            {
                var existingImages = await _uow.ProductImages.FindAsync(i => i.ProductId == dto.ProductId && i.IsPrimary, ct);
                foreach (var img in existingImages) { img.IsPrimary = false; _uow.ProductImages.Update(img); }
            }

            var image = new ProductImage
            {
                ProductId = dto.ProductId,
                ImageUrl = dto.ImageUrl,
                AltText = dto.AltText,
                IsPrimary = dto.IsPrimary,
                DisplayOrder = dto.DisplayOrder
            };
            await _uow.ProductImages.AddAsync(image, ct);
            await _uow.SaveChangesAsync(ct);

            return Result<ProductImageDto>.Success(new ProductImageDto
            {
                Id = image.Id, ProductId = image.ProductId, ImageUrl = image.ImageUrl,
                AltText = image.AltText, IsPrimary = image.IsPrimary, DisplayOrder = image.DisplayOrder
            });
        }

        public async Task<Result<ProductImageDto>> UpdateAsync(UpdateProductImageDto dto, CancellationToken ct = default)
        {
            var image = await _uow.ProductImages.GetByIdAsync(dto.Id, ct);
            if (image == null || image.IsDeleted)
                return Result<ProductImageDto>.Failure("Image not found.");

            if (dto.IsPrimary && !image.IsPrimary)
            {
                var existingImages = await _uow.ProductImages.FindAsync(i => i.ProductId == dto.ProductId && i.IsPrimary && i.Id != dto.Id, ct);
                foreach (var img in existingImages) { img.IsPrimary = false; _uow.ProductImages.Update(img); }
            }

            image.ImageUrl = dto.ImageUrl;
            image.AltText = dto.AltText;
            image.IsPrimary = dto.IsPrimary;
            image.DisplayOrder = dto.DisplayOrder;
            image.UpdatedAt = DateTime.UtcNow;
            _uow.ProductImages.Update(image);
            await _uow.SaveChangesAsync(ct);

            return Result<ProductImageDto>.Success(new ProductImageDto
            {
                Id = image.Id, ProductId = image.ProductId, ImageUrl = image.ImageUrl,
                AltText = image.AltText, IsPrimary = image.IsPrimary, DisplayOrder = image.DisplayOrder
            });
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
        {
            var image = await _uow.ProductImages.GetByIdAsync(id, ct);
            if (image == null || image.IsDeleted) return Result.Failure("Image not found.");
            image.IsDeleted = true;
            _uow.ProductImages.Update(image);
            await _uow.SaveChangesAsync(ct);
            return Result.Success();
        }

        public async Task<Result> SetPrimaryAsync(int productId, int imageId, CancellationToken ct = default)
        {
            await _uow.ProductImages.SetPrimaryImageAsync(productId, imageId, ct);
            await _uow.SaveChangesAsync(ct);
            return Result.Success();
        }
    }
}