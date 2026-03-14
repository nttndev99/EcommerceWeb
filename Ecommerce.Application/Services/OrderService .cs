using Microsoft.EntityFrameworkCore;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Order;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _uow;

    public OrderService(IUnitOfWork uow) => _uow = uow;

    // ─────────────────────────────────────────────
    // GET PAGED
    // ─────────────────────────────────────────────
    public async Task<PagedResult<OrderListDto>> GetPagedAsync(
        OrderFilterParams filter, CancellationToken ct = default)
    {
        var (orders, total) = await _uow.Orders.GetPagedAsync(filter, ct);

        var items = orders.Select(o => new OrderListDto
        {
            Id            = o.Id,
            OrderCode     = o.OrderCode,
            UserId        = o.UserId,
            OrderStatus   = o.OrderStatus,
            PaymentStatus = o.PaymentStatus,
            PaymentMethod = o.PaymentMethod,
            RecipientName = o.RecipientName,
            Total         = o.Total,
            ItemCount     = o.Items.Count,
            CreatedAt     = o.CreatedAt,
        }).ToList();

        return PagedResult<OrderListDto>.Create(items, total, filter.PageNumber, filter.PageSize);
    }

    // ─────────────────────────────────────────────
    // GET BY ID / CODE
    // ─────────────────────────────────────────────
    public async Task<OrderDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var order = await _uow.Orders.GetByIdAsync(id, ct);
        return order is null ? null : MapToDto(order);
    }

    public async Task<OrderDto?> GetByCodeAsync(string orderCode, CancellationToken ct = default)
    {
        var order = await _uow.Orders.GetByCodeAsync(orderCode, ct);
        return order is null ? null : MapToDto(order);
    }

    // ─────────────────────────────────────────────
    // CREATE
    // ─────────────────────────────────────────────
    public async Task<Result<OrderDto>> CreateAsync(
        CreateOrderDto dto, CancellationToken ct = default)
    {
        if (!dto.Items.Any())
            return Result<OrderDto>.Failure("Order must have at least one item.");

        var orderItems    = new List<OrderItem>();
        decimal subTotal  = 0;

        foreach (var line in dto.Items)
        {
            if (line.Quantity <= 0)
                return Result<OrderDto>.Failure("Quantity must be greater than 0.");

            // ── Load Product ───────────────────────
            var product = await _uow.Products.Query()
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == line.ProductId && p.Status == ProductStatus.Active, ct);

            if (product is null)
                return Result<OrderDto>.Failure($"Product #{line.ProductId} not found or inactive.");

            // ── Resolve price & variant info ───────
            decimal unitPrice   = product.SalePrice ?? product.BasePrice;
            string? variantName = null;
            string? sku         = product.SKU;
            string? imageUrl    = product.Images
                .OrderBy(i => i.DisplayOrder)
                .FirstOrDefault(i => i.IsPrimary)?.ImageUrl;

            if (line.ProductVariantId.HasValue)
            {
                var variant = await _uow.ProductVariants.Query()
                    .FirstOrDefaultAsync(v =>
                        v.Id == line.ProductVariantId.Value &&
                        v.ProductId == line.ProductId &&
                        v.IsActive, ct);

                if (variant is null)
                    return Result<OrderDto>.Failure(
                        $"Variant #{line.ProductVariantId} not found or inactive.");

                unitPrice   = variant.SalePrice ?? variant.Price;
                variantName = variant.Name;
                sku         = variant.SKU ?? sku;
                imageUrl    = variant.ImageUrl ?? imageUrl;
            }

            // ── Check Inventory ────────────────────
            var inventory = await _uow.Inventories.Query()
                .FirstOrDefaultAsync(i =>
                    i.ProductId        == line.ProductId &&
                    i.ProductVariantId == line.ProductVariantId, ct);

            if (inventory is null)
                return Result<OrderDto>.Failure(
                    $"No inventory record for '{product.Name}'.");

            if (inventory.AvailableQuantity < line.Quantity)
                return Result<OrderDto>.Failure(
                    $"Insufficient stock for '{product.Name}'" +
                    (variantName is not null ? $" ({variantName})" : "") +
                    $". Available: {inventory.AvailableQuantity}.");

            // Reserve stock
            inventory.ReservedQuantity += line.Quantity;
            inventory.LastStockUpdate   = DateTime.UtcNow;

            orderItems.Add(new OrderItem
            {
                ProductId        = line.ProductId,
                ProductVariantId = line.ProductVariantId,
                ProductName      = product.Name,
                VariantName      = variantName,
                SKU              = sku,
                ImageUrl         = imageUrl,
                Quantity         = line.Quantity,
                UnitPrice        = unitPrice,
            });

            subTotal += unitPrice * line.Quantity;
        }

        // ── Coupon ─────────────────────────────────
        decimal discount = 0;
        string? couponCode = null;

        if (!string.IsNullOrWhiteSpace(dto.CouponCode))
        {
            // Nếu có Coupon entity — validate ở đây
            // Tạm thời bỏ qua nếu chưa có Coupon entity
            couponCode = dto.CouponCode.Trim().ToUpper();
        }

        // ── Build Order ────────────────────────────
        var order = new Order
        {
            OrderCode      = await _uow.Orders.GenerateOrderCodeAsync(ct),
            UserId         = dto.UserId,
            PaymentMethod  = dto.PaymentMethod,
            RecipientName  = dto.RecipientName,
            RecipientPhone = dto.RecipientPhone,
            AddressLine    = dto.AddressLine,
            Ward           = dto.Ward,
            District       = dto.District,
            Province       = dto.Province,
            ShippingFee    = dto.ShippingFee,
            SubTotal       = subTotal,
            DiscountAmount = discount,
            Total          = subTotal + dto.ShippingFee - discount,
            CouponCode     = couponCode,
            Note           = dto.Note,
            CreatedAt      = DateTime.UtcNow,
            Items          = orderItems,
        };

        await _uow.SaveChangesAsync(ct);   // save inventory reservations
        await _uow.Orders.AddAsync(order, ct);

        return Result<OrderDto>.Success(MapToDto(order));
    }

    // ─────────────────────────────────────────────
    // UPDATE ORDER STATUS
    // ─────────────────────────────────────────────
    public async Task<Result<OrderDto>> UpdateOrderStatusAsync(
        UpdateOrderStatusDto dto, CancellationToken ct = default)
    {
        var order = await _uow.Orders.GetByIdAsync(dto.Id, ct);
        if (order is null)
            return Result<OrderDto>.Failure($"Order #{dto.Id} not found.");

        // ── Validate transition ────────────────────
        var allowed = order.OrderStatus switch
        {
            OrderStatus.Pending    => new[] { OrderStatus.Processing, OrderStatus.Cancelled },
            OrderStatus.Processing => new[] { OrderStatus.Shipped,    OrderStatus.Cancelled },
            OrderStatus.Shipped    => new[] { OrderStatus.Delivered,  OrderStatus.Cancelled },
            _                      => Array.Empty<OrderStatus>(),
        };

        if (!allowed.Contains(dto.OrderStatus))
            return Result<OrderDto>.Failure(
                $"Cannot change status from '{order.OrderStatus}' to '{dto.OrderStatus}'.");

        // ── Cancelled → release reserved stock ─────
        if (dto.OrderStatus == OrderStatus.Cancelled)
        {
            foreach (var item in order.Items)
            {
                var inv = await _uow.Inventories.Query()
                    .FirstOrDefaultAsync(i =>
                        i.ProductId        == item.ProductId &&
                        i.ProductVariantId == item.ProductVariantId, ct);

                if (inv is not null)
                {
                    inv.ReservedQuantity = Math.Max(0, inv.ReservedQuantity - item.Quantity);
                    inv.LastStockUpdate  = DateTime.UtcNow;
                }
            }

            order.CancelReason = dto.CancelReason;
            order.CancelledAt  = DateTime.UtcNow;
        }

        // ── Delivered → deduct stock permanently ───
        if (dto.OrderStatus == OrderStatus.Delivered)
        {
            foreach (var item in order.Items)
            {
                var inv = await _uow.Inventories.Query()
                    .FirstOrDefaultAsync(i =>
                        i.ProductId        == item.ProductId &&
                        i.ProductVariantId == item.ProductVariantId, ct);

                if (inv is not null)
                {
                    inv.ReservedQuantity = Math.Max(0, inv.ReservedQuantity - item.Quantity);
                    inv.Quantity         = Math.Max(0, inv.Quantity         - item.Quantity);
                    inv.LastStockUpdate  = DateTime.UtcNow;
                }
            }

            order.DeliveredAt = DateTime.UtcNow;
        }

        if (dto.OrderStatus == OrderStatus.Shipped)
            order.ShippedAt = DateTime.UtcNow;

        if (dto.OrderStatus == OrderStatus.Processing && order.PaymentMethod != PaymentMethod.COD)
        {
            // auto-mark paid for non-COD when processing
        }

        order.OrderStatus = dto.OrderStatus;

        await _uow.SaveChangesAsync(ct);
        await _uow.Orders.UpdateAsync(order, ct);

        return Result<OrderDto>.Success(MapToDto(order));
    }

    // ─────────────────────────────────────────────
    // UPDATE PAYMENT STATUS
    // ─────────────────────────────────────────────
    public async Task<Result<OrderDto>> UpdatePaymentStatusAsync(
        UpdatePaymentStatusDto dto, CancellationToken ct = default)
    {
        var order = await _uow.Orders.GetByIdAsync(dto.Id, ct);
        if (order is null)
            return Result<OrderDto>.Failure($"Order #{dto.Id} not found.");

        order.PaymentStatus = dto.PaymentStatus;

        if (dto.PaymentStatus == PaymentStatus.Paid)
            order.PaidAt = DateTime.UtcNow;

        await _uow.Orders.UpdateAsync(order, ct);
        return Result<OrderDto>.Success(MapToDto(order));
    }

    // ─────────────────────────────────────────────
    // CANCEL (shortcut)
    // ─────────────────────────────────────────────
    public Task<Result<OrderDto>> CancelAsync(int id, string reason, CancellationToken ct = default)
        => UpdateOrderStatusAsync(new UpdateOrderStatusDto
        {
            Id           = id,
            OrderStatus  = OrderStatus.Cancelled,
            CancelReason = reason,
        }, ct);

    // ─────────────────────────────────────────────
    // COUNT
    // ─────────────────────────────────────────────
    public Task<int> CountByStatusAsync(OrderStatus? status = null, CancellationToken ct = default)
        => _uow.Orders.CountByStatusAsync(status, ct);

    // ─────────────────────────────────────────────
    // MAPPER
    // ─────────────────────────────────────────────
    private static OrderDto MapToDto(Order o) => new()
    {
        Id             = o.Id,
        OrderCode      = o.OrderCode,
        UserId         = o.UserId,
        OrderStatus    = o.OrderStatus,
        PaymentStatus  = o.PaymentStatus,
        PaymentMethod  = o.PaymentMethod,
        RecipientName  = o.RecipientName,
        RecipientPhone = o.RecipientPhone,
        AddressLine    = o.AddressLine,
        Ward           = o.Ward,
        District       = o.District,
        Province       = o.Province,
        SubTotal       = o.SubTotal,
        ShippingFee    = o.ShippingFee,
        DiscountAmount = o.DiscountAmount,
        Total          = o.Total,
        CouponCode     = o.CouponCode,
        CreatedAt      = o.CreatedAt,
        PaidAt         = o.PaidAt,
        ShippedAt      = o.ShippedAt,
        DeliveredAt    = o.DeliveredAt,
        CancelledAt    = o.CancelledAt,
        CancelReason   = o.CancelReason,
        Note           = o.Note,
        Items          = o.Items.Select(i => new OrderItemDto
        {
            Id               = i.Id,
            ProductId        = i.ProductId,
            ProductVariantId = i.ProductVariantId,
            ProductName      = i.ProductName,
            VariantName      = i.VariantName,
            SKU              = i.SKU,
            ImageUrl         = i.ImageUrl,
            Quantity         = i.Quantity,
            UnitPrice        = i.UnitPrice,
            TotalPrice       = i.TotalPrice,
        }).ToList(),
    };
}