using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Persistence
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Slug).IsRequired().HasMaxLength(200);
            builder.HasIndex(c => c.Slug).IsUnique();
            builder.Property(c => c.Description).HasMaxLength(2000);
            builder.Property(c => c.ImageUrl).HasMaxLength(500);

            builder.HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }

    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(300);
            builder.Property(p => p.Slug).IsRequired().HasMaxLength(300);
            builder.HasIndex(p => p.Slug).IsUnique();
            builder.Property(p => p.BasePrice).HasColumnType("decimal(18,2)");
            builder.Property(p => p.SalePrice).HasColumnType("decimal(18,2)");
            builder.Property(p => p.Weight).HasColumnType("decimal(10,3)");
            builder.Property(p => p.SKU).HasMaxLength(100);
            builder.Property(p => p.Brand).HasMaxLength(200);
            builder.Property(p => p.Tags).HasMaxLength(500);

            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index for performance
            builder.HasIndex(p => p.CategoryId);
            builder.HasIndex(p => p.Status);
            builder.HasIndex(p => p.CreatedAt);
            builder.HasIndex(p => new { p.CategoryId, p.Status });

            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }

    public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.ToTable("ProductVariants");
            builder.HasKey(v => v.Id);
            builder.Property(v => v.Name).IsRequired().HasMaxLength(200);
            builder.Property(v => v.SKU).HasMaxLength(100);
            builder.HasIndex(v => v.SKU).IsUnique().HasFilter("[SKU] IS NOT NULL");
            builder.Property(v => v.Price).HasColumnType("decimal(18,2)");
            builder.Property(v => v.SalePrice).HasColumnType("decimal(18,2)");
            builder.Property(v => v.Color).HasMaxLength(100);
            builder.Property(v => v.Size).HasMaxLength(50);
            builder.Property(v => v.Material).HasMaxLength(200);
            builder.Property(v => v.ImageUrl).HasMaxLength(500);

            builder.HasOne(v => v.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(v => v.ProductId);
            builder.HasQueryFilter(v => !v.IsDeleted);
        }
    }

    public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.ToTable("ProductImages");
            builder.HasKey(i => i.Id);
            builder.Property(i => i.ImageUrl).IsRequired().HasMaxLength(500);
            builder.Property(i => i.AltText).HasMaxLength(300);

            builder.HasOne(i => i.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(i => i.ProductId);
            builder.HasQueryFilter(i => !i.IsDeleted);
        }
    }

    public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
    {
        public void Configure(EntityTypeBuilder<Inventory> builder)
        {
            builder.ToTable("Inventories");
            builder.HasKey(i => i.Id);
            builder.Property(i => i.WarehouseLocation).HasMaxLength(200);

            builder.HasOne(i => i.Product)
                .WithMany(p => p.Inventories)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.ProductVariant)
                .WithMany(v => v.Inventories)
                .HasForeignKey(i => i.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(i => i.ProductId);
            builder.HasIndex(i => new { i.ProductId, i.ProductVariantId }).IsUnique();

            // Ignore computed properties
            builder.Ignore(i => i.AvailableQuantity);
            builder.Ignore(i => i.IsLowStock);
            builder.Ignore(i => i.IsOutOfStock);

            builder.HasQueryFilter(i => !i.IsDeleted);
        }
    }

}