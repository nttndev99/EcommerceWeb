using System.ComponentModel.DataAnnotations;
using Ecommerce.Application.Common;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.DTOs.Order;

public class OrderDto
{
    public int           Id            { get; set; }
    public string        OrderCode     { get; set; } = string.Empty;
    public string        UserId        { get; set; } = string.Empty;
    public OrderStatus   OrderStatus   { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string        RecipientName  { get; set; } = string.Empty;
    public string        RecipientPhone { get; set; } = string.Empty;
    public string        AddressLine    { get; set; } = string.Empty;
    public string        Ward           { get; set; } = string.Empty;
    public string        District       { get; set; } = string.Empty;
    public string        Province       { get; set; } = string.Empty;
    public string        FullAddress    => $"{AddressLine}, {Ward}, {District}, {Province}";
    public decimal       SubTotal       { get; set; }
    public decimal       ShippingFee    { get; set; }
    public decimal       DiscountAmount { get; set; }
    public decimal       Total          { get; set; }
    public string?       CouponCode     { get; set; }
    public DateTime      CreatedAt      { get; set; }
    public DateTime?     PaidAt         { get; set; }
    public DateTime?     ShippedAt      { get; set; }
    public DateTime?     DeliveredAt    { get; set; }
    public DateTime?     CancelledAt    { get; set; }
    public string?       CancelReason   { get; set; }
    public string?       Note           { get; set; }
    public List<OrderItemDto> Items     { get; set; } = new();
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

public class OrderStatDto
{
    public string  UserId      { get; set; } = string.Empty;
    public int     TotalOrders { get; set; }
    public decimal TotalSpent  { get; set; }
}

// CREATE
public class CreateOrderDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Recipient name is required.")]
    [MaxLength(100, ErrorMessage = "Name must be at most 100 characters.")]
    public string RecipientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [Phone(ErrorMessage = "Invalid phone number.")]
    [MaxLength(20)]
    public string RecipientPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required.")]
    [MaxLength(255)]
    public string AddressLine { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ward is required.")]
    [MaxLength(100)]
    public string Ward { get; set; } = string.Empty;

    [Required(ErrorMessage = "District is required.")]
    [MaxLength(100)]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "Province is required.")]
    [MaxLength(100)]
    public string Province { get; set; } = string.Empty;

    [EnumDataType(typeof(PaymentMethod), ErrorMessage = "Invalid payment method.")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;

    [MaxLength(50)]
    public string? CouponCode { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Shipping fee cannot be negative.")]
    public decimal ShippingFee { get; set; }

    [MaxLength(1000)]
    public string? Note { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Order must have at least one item.")]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public class CreateOrderItemDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Product ID must be valid.")]
    public int ProductId { get; set; }

    public int? ProductVariantId { get; set; }

    [Required]
    [Range(1, 9999, ErrorMessage = "Quantity must be between 1 and 9999.")]
    public int Quantity { get; set; }
}

// UPDATE
public class UpdateOrderStatusDto
{
    [Required]
    public int Id { get; set; }

    [EnumDataType(typeof(OrderStatus), ErrorMessage = "Invalid order status.")]
    public OrderStatus OrderStatus { get; set; }

    [MaxLength(500, ErrorMessage = "Cancel reason must be at most 500 characters.")]
    public string? CancelReason { get; set; }
}

public class UpdatePaymentStatusDto
{
    [Required]
    public int Id { get; set; }

    [EnumDataType(typeof(PaymentStatus), ErrorMessage = "Invalid payment status.")]
    public PaymentStatus PaymentStatus { get; set; }
}

// FILTER
public class OrderFilterParams : PaginationParams
{
    [MaxLength(100)]
    public string? Search { get; set; }

    public string?        UserId        { get; set; }
    public OrderStatus?   OrderStatus   { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public DateTime?      From          { get; set; }
    public DateTime?      To            { get; set; }
    public string         SortBy        { get; set; } = "createdAt";
    public string         SortDirection { get; set; } = "desc";
}