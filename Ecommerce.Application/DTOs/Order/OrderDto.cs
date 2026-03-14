using Ecommerce.Application.Common;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.DTOs.Order;

// ─────────────────────────────────────────────
// READ
// ─────────────────────────────────────────────
public class OrderDto
{
    public int           Id            { get; set; }
    public string        OrderCode     { get; set; } = string.Empty;
    public string        UserId        { get; set; } = string.Empty;
    public OrderStatus   OrderStatus   { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public PaymentMethod PaymentMethod { get; set; }

    // Shipping
    public string RecipientName  { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string AddressLine    { get; set; } = string.Empty;
    public string Ward           { get; set; } = string.Empty;
    public string District       { get; set; } = string.Empty;
    public string Province       { get; set; } = string.Empty;
    public string FullAddress    => $"{AddressLine}, {Ward}, {District}, {Province}";

    // Pricing
    public decimal SubTotal       { get; set; }
    public decimal ShippingFee    { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total          { get; set; }
    public string? CouponCode     { get; set; }

    // Timestamps
    public DateTime  CreatedAt   { get; set; }
    public DateTime? PaidAt      { get; set; }
    public DateTime? ShippedAt   { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string?   CancelReason { get; set; }
    public string?   Note         { get; set; }

    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderListDto
{
    public int           Id            { get; set; }
    public string        OrderCode     { get; set; } = string.Empty;
    public string        UserId        { get; set; } = string.Empty;
    public OrderStatus   OrderStatus   { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string        RecipientName { get; set; } = string.Empty;
    public decimal       Total         { get; set; }
    public int           ItemCount     { get; set; }
    public DateTime      CreatedAt     { get; set; }
}

public class OrderItemDto
{
    public int     Id               { get; set; }
    public int     ProductId        { get; set; }
    public int?    ProductVariantId { get; set; }
    public string  ProductName      { get; set; } = string.Empty;
    public string? VariantName      { get; set; }
    public string? SKU              { get; set; }
    public string? ImageUrl         { get; set; }
    public int     Quantity         { get; set; }
    public decimal UnitPrice        { get; set; }
    public decimal TotalPrice       { get; set; }
}

// ─────────────────────────────────────────────
// CREATE
// ─────────────────────────────────────────────
public class CreateOrderDto
{
    public string UserId { get; set; } = string.Empty;

    public string RecipientName  { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string AddressLine    { get; set; } = string.Empty;
    public string Ward           { get; set; } = string.Empty;
    public string District       { get; set; } = string.Empty;
    public string Province       { get; set; } = string.Empty;

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;
    public string?       CouponCode   { get; set; }
    public decimal       ShippingFee  { get; set; }
    public string?       Note         { get; set; }

    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public class CreateOrderItemDto
{
    public int  ProductId        { get; set; }
    public int? ProductVariantId { get; set; }
    public int  Quantity         { get; set; }
}

// ─────────────────────────────────────────────
// UPDATE
// ─────────────────────────────────────────────
public class UpdateOrderStatusDto
{
    public int         Id           { get; set; }
    public OrderStatus OrderStatus  { get; set; }
    public string?     CancelReason { get; set; }
}

public class UpdatePaymentStatusDto
{
    public int           Id            { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
}

// ─────────────────────────────────────────────
// FILTER
// ─────────────────────────────────────────────
public class OrderFilterParams : PaginationParams
{
    public string?        Search        { get; set; }
    public string?        UserId        { get; set; }
    public OrderStatus?   OrderStatus   { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public DateTime?      From          { get; set; }
    public DateTime?      To            { get; set; }
    public string         SortBy        { get; set; } = "createdAt";
    public string         SortDirection { get; set; } = "desc";
}