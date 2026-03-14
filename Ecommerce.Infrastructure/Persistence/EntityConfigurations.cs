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

    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");
    
            builder.HasKey(o => o.Id);
    
            builder.Property(o => o.OrderCode)
                .IsRequired()
                .HasMaxLength(30);
    
            builder.HasIndex(o => o.OrderCode)
                .IsUnique();
    
            builder.Property(o => o.UserId)
                .IsRequired()
                .HasMaxLength(450);
    
            // ── Decimal precision ──────────────────────
            builder.Property(o => o.SubTotal)
                .HasPrecision(18, 2);
    
            builder.Property(o => o.ShippingFee)
                .HasPrecision(18, 2);
    
            builder.Property(o => o.DiscountAmount)
                .HasPrecision(18, 2);
    
            builder.Property(o => o.Total)
                .HasPrecision(18, 2);
    
            // ── String lengths ─────────────────────────
            builder.Property(o => o.RecipientName).HasMaxLength(100);
            builder.Property(o => o.RecipientPhone).HasMaxLength(20);
            builder.Property(o => o.AddressLine).HasMaxLength(255);
            builder.Property(o => o.Ward).HasMaxLength(100);
            builder.Property(o => o.District).HasMaxLength(100);
            builder.Property(o => o.Province).HasMaxLength(100);
            builder.Property(o => o.CouponCode).HasMaxLength(50);
            builder.Property(o => o.CancelReason).HasMaxLength(500);
            builder.Property(o => o.Note).HasMaxLength(1000);
    
            // ── Navigation ─────────────────────────────
            builder.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");
    
            builder.HasKey(i => i.Id);
    
            // ── Decimal precision ──────────────────────
            builder.Property(i => i.UnitPrice)
                .HasPrecision(18, 2);
    
            // TotalPrice là computed property — ignore
            builder.Ignore(i => i.TotalPrice);
    
            // ── String lengths ─────────────────────────
            builder.Property(i => i.ProductName).HasMaxLength(255);
            builder.Property(i => i.VariantName).HasMaxLength(100);
            builder.Property(i => i.SKU).HasMaxLength(100);
            builder.Property(i => i.ImageUrl).HasMaxLength(500);
    
            // ── Fix: global query filter warning ──────
            // Product có global filter (IsDeleted), cần configure navigation optional
            // hoặc dùng NoAction để tránh conflict với soft-delete filter
            builder.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);   // ← optional navigation = suppress EF warning 10622
    
            builder.HasOne(i => i.ProductVariant)
                .WithMany()
                .HasForeignKey(i => i.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }
    }    


}