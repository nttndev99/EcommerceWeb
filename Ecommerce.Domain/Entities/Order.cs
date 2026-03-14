using Ecommerce.Domain.Common;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities;

public class Order : BaseEntity
{
    // ── Identity ──────────────────────────────────
    public string  OrderCode { get; set; } = string.Empty;   // ORD-20240101-0001
    public string  UserId    { get; set; } = string.Empty;

    // ── Status ────────────────────────────────────
    public OrderStatus   OrderStatus   { get; set; } = OrderStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;

    // ── Shipping address ──────────────────────────
    public string RecipientName  { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string AddressLine    { get; set; } = string.Empty;
    public string Ward           { get; set; } = string.Empty;
    public string District       { get; set; } = string.Empty;
    public string Province       { get; set; } = string.Empty;

    // ── Pricing ───────────────────────────────────
    public decimal SubTotal       { get; set; }
    public decimal ShippingFee    { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total          { get; set; }

    // ── Coupon ────────────────────────────────────
    public string? CouponCode { get; set; }

    // ── Timestamps ────────────────────────────────
    public DateTime? PaidAt      { get; set; }
    public DateTime? ShippedAt   { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public string? CancelReason { get; set; }
    public string? Note         { get; set; }

    // ── Navigation ────────────────────────────────
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

