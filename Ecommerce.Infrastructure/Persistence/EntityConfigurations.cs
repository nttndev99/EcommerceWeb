using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Infrastructure.Persistence.Configurations;

// ─────────────────────────────────────────────────────────────────────────────
// CATEGORY
// ─────────────────────────────────────────────────────────────────────────────
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.ToTable("Categories");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).IsRequired().HasMaxLength(100);
        b.Property(x => x.Slug).IsRequired().HasMaxLength(120);
        b.Property(x => x.Description).HasMaxLength(500);
        b.Property(x => x.ImageUrl).HasMaxLength(500);

        b.HasIndex(x => x.Slug).IsUnique();

        // Global query filter — soft delete
        b.HasQueryFilter(x => !x.IsDeleted);

        // Self-referencing: Category → Parent
        b.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// PRODUCT
// ─────────────────────────────────────────────────────────────────────────────
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.ToTable("Products");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).IsRequired().HasMaxLength(255);
        b.Property(x => x.Slug).IsRequired().HasMaxLength(300);
        b.Property(x => x.Description).HasMaxLength(4000);
        b.Property(x => x.ShortDescription).HasMaxLength(500);
        b.Property(x => x.SKU).HasMaxLength(100);
        b.Property(x => x.Brand).HasMaxLength(100);
        b.Property(x => x.Tags).HasMaxLength(500);

        b.Property(x => x.BasePrice).HasPrecision(18, 2);
        b.Property(x => x.SalePrice).HasPrecision(18, 2);
        b.Property(x => x.Weight).HasPrecision(10, 3);

        b.HasIndex(x => x.Slug).IsUnique();
        b.HasIndex(x => x.SKU).IsUnique().HasFilter("[SKU] IS NOT NULL");

        b.HasQueryFilter(x => !x.IsDeleted);

        // Product → Category
        b.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Product → Variants (cascade)
        b.HasMany(x => x.Variants)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Product → Images (cascade)
        b.HasMany(x => x.Images)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Product → Inventories (cascade)
        b.HasMany(x => x.Inventories)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// PRODUCT VARIANT
// ─────────────────────────────────────────────────────────────────────────────
public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> b)
    {
        b.ToTable("ProductVariants");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).IsRequired().HasMaxLength(200);
        b.Property(x => x.SKU).HasMaxLength(100);
        b.Property(x => x.Color).HasMaxLength(50);
        b.Property(x => x.Size).HasMaxLength(50);
        b.Property(x => x.Material).HasMaxLength(100);
        b.Property(x => x.ImageUrl).HasMaxLength(500);

        b.Property(x => x.Price).HasPrecision(18, 2);
        b.Property(x => x.SalePrice).HasPrecision(18, 2);

        b.HasIndex(x => x.SKU).IsUnique().HasFilter("[SKU] IS NOT NULL");

        b.HasQueryFilter(x => !x.IsDeleted);

        // Variant → Inventories (cascade)
        b.HasMany(x => x.Inventories)
            .WithOne(x => x.ProductVariant)
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .IsRequired(false);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// PRODUCT IMAGE
// ─────────────────────────────────────────────────────────────────────────────
public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> b)
    {
        b.ToTable("ProductImages");
        b.HasKey(x => x.Id);

        b.Property(x => x.ImageUrl).IsRequired().HasMaxLength(500);
        b.Property(x => x.AltText).HasMaxLength(200);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// INVENTORY
// ─────────────────────────────────────────────────────────────────────────────
public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> b)
    {
        b.ToTable("Inventories");
        b.HasKey(x => x.Id);

        b.Property(x => x.WarehouseLocation).HasMaxLength(100);

        // Ignore computed properties
        b.Ignore(x => x.AvailableQuantity);
        b.Ignore(x => x.IsLowStock);
        b.Ignore(x => x.IsOutOfStock);

        // Unique: 1 inventory record per (Product, Variant)
        b.HasIndex(x => new { x.ProductId, x.ProductVariantId }).IsUnique();

        // Inventory → Product (no cascade — product already cascades)
        b.HasOne(x => x.Product)
            .WithMany(x => x.Inventories)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.NoAction);

        // Inventory → ProductVariant (optional)
        b.HasOne(x => x.ProductVariant)
            .WithMany(x => x.Inventories)
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ORDER
// ─────────────────────────────────────────────────────────────────────────────
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("Orders");
        b.HasKey(x => x.Id);

        b.Property(x => x.OrderCode).IsRequired().HasMaxLength(30);
        b.Property(x => x.UserId).IsRequired().HasMaxLength(450);

        b.Property(x => x.RecipientName).HasMaxLength(100);
        b.Property(x => x.RecipientPhone).HasMaxLength(20);
        b.Property(x => x.AddressLine).HasMaxLength(255);
        b.Property(x => x.Ward).HasMaxLength(100);
        b.Property(x => x.District).HasMaxLength(100);
        b.Property(x => x.Province).HasMaxLength(100);
        b.Property(x => x.CouponCode).HasMaxLength(50);
        b.Property(x => x.CancelReason).HasMaxLength(500);
        b.Property(x => x.Note).HasMaxLength(1000);

        b.Property(x => x.SubTotal).HasPrecision(18, 2);
        b.Property(x => x.ShippingFee).HasPrecision(18, 2);
        b.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        b.Property(x => x.Total).HasPrecision(18, 2);

        b.HasIndex(x => x.OrderCode).IsUnique();
        b.HasIndex(x => x.UserId);

        // Order → Items (cascade)
        b.HasMany(x => x.Items)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ORDER ITEM
// ─────────────────────────────────────────────────────────────────────────────
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> b)
    {
        b.ToTable("OrderItems");
        b.HasKey(x => x.Id);

        b.Property(x => x.ProductName).IsRequired().HasMaxLength(255);
        b.Property(x => x.VariantName).HasMaxLength(100);
        b.Property(x => x.SKU).HasMaxLength(100);
        b.Property(x => x.ImageUrl).HasMaxLength(500);

        b.Property(x => x.UnitPrice).HasPrecision(18, 2);

        // TotalPrice is computed — ignore
        b.Ignore(x => x.TotalPrice);

        // Fix: Product has global query filter → make navigation optional
        // to suppress EF warning 10622
        b.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        b.HasOne(x => x.ProductVariant)
            .WithMany()
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// APP USER
// ─────────────────────────────────────────────────────────────────────────────
public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> b)
    {
        b.Property(x => x.FullName).IsRequired().HasMaxLength(100);
        b.Property(x => x.AvatarUrl).HasMaxLength(500);
        b.Property(x => x.Gender).HasMaxLength(10);
        b.Property(x => x.AddressLine).HasMaxLength(255);
        b.Property(x => x.Ward).HasMaxLength(100);
        b.Property(x => x.District).HasMaxLength(100);
        b.Property(x => x.Province).HasMaxLength(100);

        // AppUser → Orders
        b.HasMany(x => x.Orders)
            .WithOne()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}