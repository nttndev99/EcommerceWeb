using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Cart;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly IHttpContextAccessor _http;
    private readonly IUnitOfWork          _uow;

    private const string SessionKey = "Cart";

    public CartService(IHttpContextAccessor http, IUnitOfWork uow)
    {
        _http = http;
        _uow  = uow;
    }

    private HttpContext Http      => _http.HttpContext!;
    private string?    UserId     => Http.User.FindFirstValue(ClaimTypes.NameIdentifier);

    // ─────────────────────────────────────────────
    // GET CART
    // ─────────────────────────────────────────────
    public async Task<CartDto> GetCartAsync(CancellationToken ct = default)
    {
        var cart = LoadFromSession() ?? new CartDto();
        cart.UserId = UserId;
        await SyncPricesAndStockAsync(cart, ct);
        return cart;
    }

    // ─────────────────────────────────────────────
    // ADD TO CART
    // ─────────────────────────────────────────────
    public async Task<Result> AddToCartAsync(AddToCartDto dto, CancellationToken ct = default)
    {
        var product = await _uow.Products.GetByIdAsync(dto.ProductId, ct);
        if (product is null || product.Status != ProductStatus.Active || product.IsDeleted)
            return Result.Failure("Product not found or unavailable.");

        decimal unitPrice   = product.SalePrice ?? product.BasePrice;
        decimal origPrice   = product.BasePrice;
        string? variantName = null;
        string? sku         = product.SKU;
        string? imageUrl    = product.Images?
            .OrderBy(i => i.DisplayOrder)
            .FirstOrDefault(i => i.IsPrimary)?.ImageUrl;

        if (dto.ProductVariantId.HasValue)
        {
            var variant = await _uow.ProductVariants.GetByIdAsync(dto.ProductVariantId.Value, ct);
            if (variant is null || variant.ProductId != dto.ProductId || !variant.IsActive)
                return Result.Failure("Variant not found or inactive.");

            unitPrice   = variant.SalePrice ?? variant.Price;
            origPrice   = variant.Price;
            variantName = variant.Name;
            sku         = variant.SKU ?? sku;
            imageUrl    = variant.ImageUrl ?? imageUrl;
        }

        var inventory = await _uow.Inventories
            .GetByProductAndVariantAsync(dto.ProductId, dto.ProductVariantId, ct);

        int maxStock = inventory?.AvailableQuantity ?? 0;
        if (maxStock <= 0)
            return Result.Failure("This item is out of stock.");

        var cart    = LoadFromSession() ?? new CartDto();
        var itemKey = BuildItemKey(dto.ProductId, dto.ProductVariantId);
        var existing = cart.Items.FirstOrDefault(i => i.CartItemId == itemKey);

        if (existing is not null)
        {
            int newQty = existing.Quantity + dto.Quantity;
            if (newQty > maxStock)
                return Result.Failure($"Only {maxStock} units available.");
            existing.Quantity = newQty;
        }
        else
        {
            if (dto.Quantity > maxStock)
                return Result.Failure($"Only {maxStock} units available.");

            cart.Items.Add(new CartItemDto
            {
                CartItemId       = itemKey,
                ProductId        = dto.ProductId,
                ProductVariantId = dto.ProductVariantId,
                ProductName      = product.Name,
                VariantName      = variantName,
                SKU              = sku,
                ImageUrl         = imageUrl,
                UnitPrice        = unitPrice,
                OriginalPrice    = origPrice,
                Quantity         = dto.Quantity,
                MaxStock         = maxStock,
                IsAvailable      = true,
            });
        }

        SaveToSession(cart);
        return Result.Success();
    }

    // ─────────────────────────────────────────────
    // UPDATE QUANTITY
    // ─────────────────────────────────────────────
    public async Task<Result> UpdateQuantityAsync(UpdateCartItemDto dto, CancellationToken ct = default)
    {
        var cart = LoadFromSession() ?? new CartDto();
        var item = cart.Items.FirstOrDefault(i => i.CartItemId == dto.CartItemId);
        if (item is null) return Result.Failure("Item not found in cart.");

        var inventory = await _uow.Inventories
            .GetByProductAndVariantAsync(item.ProductId, item.ProductVariantId, ct);

        int available = inventory?.AvailableQuantity ?? 0;
        if (dto.Quantity > available)
            return Result.Failure($"Only {available} units available.");

        item.Quantity = dto.Quantity;
        SaveToSession(cart);
        return Result.Success();
    }

    // ─────────────────────────────────────────────
    // REMOVE ITEM
    // ─────────────────────────────────────────────
    public Task<Result> RemoveItemAsync(string cartItemId, CancellationToken ct = default)
    {
        var cart = LoadFromSession() ?? new CartDto();
        cart.Items.RemoveAll(i => i.CartItemId == cartItemId);
        SaveToSession(cart);
        return Task.FromResult(Result.Success());
    }

    // ─────────────────────────────────────────────
    // CLEAR
    // ─────────────────────────────────────────────
    public Task ClearCartAsync(CancellationToken ct = default)
    {
        Http.Session.Remove(SessionKey);
        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────
    // APPLY COUPON
    // ─────────────────────────────────────────────
    public async Task<Result> ApplyCouponAsync(string code, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure("Coupon code is required.");

        var cart = LoadFromSession() ?? new CartDto();

        // TODO: replace with _uow.Coupons.GetByCodeAsync(code) when ready
        decimal discount = code.ToUpper() switch
        {
            "SAVE10"  => Math.Round(cart.SubTotal * 0.10m, 0),
            "FLAT50K" => 50_000m,
            _         => -1m,
        };

        if (discount < 0)
            return Result.Failure($"Coupon '{code}' is invalid or expired.");

        cart.CouponCode     = code.ToUpper();
        cart.DiscountAmount = discount;
        SaveToSession(cart);
        return Result.Success();
    }

    // ─────────────────────────────────────────────
    // REMOVE COUPON
    // ─────────────────────────────────────────────
    public Task RemoveCouponAsync(CancellationToken ct = default)
    {
        var cart = LoadFromSession() ?? new CartDto();
        cart.CouponCode     = null;
        cart.DiscountAmount = 0;
        SaveToSession(cart);
        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────
    // CALCULATE SHIPPING
    // ─────────────────────────────────────────────
    public Task<decimal> CalculateShippingAsync(string province, CancellationToken ct = default)
    {
        decimal fee = province?.ToLower().Trim() switch
        {
            "hà nội"      => 20_000m,
            "hồ chí minh" => 20_000m,
            "đà nẵng"     => 25_000m,
            _             => 35_000m,
        };
        return Task.FromResult(fee);
    }

    // ─────────────────────────────────────────────
    // MERGE SESSION → USER CART (after login)
    // ─────────────────────────────────────────────
    public Task MergeSessionCartAsync(string userId, CancellationToken ct = default)
    {
        var cart = LoadFromSession();
        if (cart is null) return Task.CompletedTask;
        cart.UserId = userId;
        SaveToSession(cart);
        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────
    // PRIVATE HELPERS
    // ─────────────────────────────────────────────
    private CartDto? LoadFromSession()
    {
        var json = Http.Session.GetString(SessionKey);
        return string.IsNullOrEmpty(json)
            ? null
            : JsonSerializer.Deserialize<CartDto>(json);
    }

    private void SaveToSession(CartDto cart)
        => Http.Session.SetString(SessionKey, JsonSerializer.Serialize(cart));

    private static string BuildItemKey(int productId, int? variantId)
        => variantId.HasValue ? $"{productId}-{variantId}" : $"{productId}";

    private async Task SyncPricesAndStockAsync(CartDto cart, CancellationToken ct)
    {
        if (!cart.Items.Any()) return;

        foreach (var item in cart.Items)
        {
            var inv = await _uow.Inventories
                .GetByProductAndVariantAsync(item.ProductId, item.ProductVariantId, ct);

            item.MaxStock    = inv?.AvailableQuantity ?? 0;
            item.IsAvailable = item.MaxStock > 0;

            if (item.Quantity > item.MaxStock)
                item.Quantity = Math.Max(item.MaxStock, 0);
        }

        SaveToSession(cart);
    }
}