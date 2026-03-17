using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.DTOs.Cart;

// ─────────────────────────────────────────────
// CART ITEM
// ─────────────────────────────────────────────
public class CartItemDto
{
    public string  CartItemId       { get; set; } = string.Empty; // unique key in cart
    public int     ProductId        { get; set; }
    public int?    ProductVariantId { get; set; }
    public string  ProductName      { get; set; } = string.Empty;
    public string? VariantName      { get; set; }
    public string? SKU              { get; set; }
    public string? ImageUrl         { get; set; }
    public decimal UnitPrice        { get; set; }
    public decimal OriginalPrice    { get; set; }   // before sale
    public int     Quantity         { get; set; }
    public decimal TotalPrice       => UnitPrice * Quantity;
    public int     MaxStock         { get; set; }
    public bool    IsAvailable      { get; set; } = true;
}

// ─────────────────────────────────────────────
// CART
// ─────────────────────────────────────────────
public class CartDto
{
    public string            SessionId    { get; set; } = string.Empty;
    public string?           UserId       { get; set; }
    public List<CartItemDto> Items        { get; set; } = new();
    public int               TotalItems   => Items.Sum(i => i.Quantity);
    public decimal           SubTotal     => Items.Sum(i => i.TotalPrice);
    public decimal           ShippingFee  { get; set; }
    public decimal           DiscountAmount { get; set; }
    public string?           CouponCode   { get; set; }
    public decimal           Total        => SubTotal + ShippingFee - DiscountAmount;
    public bool              IsEmpty      => !Items.Any();
}

// ─────────────────────────────────────────────
// ADD TO CART
// ─────────────────────────────────────────────
public class AddToCartDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    public int? ProductVariantId { get; set; }

    [Required]
    [Range(1, 999, ErrorMessage = "Quantity must be between 1 and 999.")]
    public int Quantity { get; set; } = 1;
}

// ─────────────────────────────────────────────
// UPDATE QUANTITY
// ─────────────────────────────────────────────
public class UpdateCartItemDto
{
    [Required]
    public string CartItemId { get; set; } = string.Empty;

    [Required]
    [Range(1, 999, ErrorMessage = "Quantity must be between 1 and 999.")]
    public int Quantity { get; set; }
}

// ─────────────────────────────────────────────
// APPLY COUPON
// ─────────────────────────────────────────────
public class ApplyCouponDto
{
    [Required(ErrorMessage = "Coupon code is required.")]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;
}

// ─────────────────────────────────────────────
// CHECKOUT
// ─────────────────────────────────────────────
public class CheckoutDto
{
    [Required(ErrorMessage = "Recipient name is required.")]
    [MaxLength(100)]
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

    [Required(ErrorMessage = "Payment method is required.")]
    public string PaymentMethod { get; set; } = "COD";

    [MaxLength(50)]
    public string? CouponCode { get; set; }

    [MaxLength(1000)]
    public string? Note { get; set; }

    // Pre-filled from cart
    public CartDto? Cart { get; set; }
}

// ─────────────────────────────────────────────
// ORDER CONFIRMATION
// ─────────────────────────────────────────────
public class OrderConfirmationDto
{
    public int      OrderId   { get; set; }
    public string   OrderCode { get; set; } = string.Empty;
    public decimal  Total     { get; set; }
    public string   Email     { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}