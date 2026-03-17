using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.Application.DTOs.Cart;
using Ecommerce.Application.DTOs.Order;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Application.Interfaces.Services;

namespace Ecommerce.MVC.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly ICartService         _cartService;
    private readonly IOrderService        _orderService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService        _emailService;

    public CheckoutController(
        ICartService         cartService,
        IOrderService        orderService,
        UserManager<AppUser> userManager,
        IEmailService        emailService)
    {
        _cartService  = cartService;
        _orderService = orderService;
        _userManager  = userManager;
        _emailService = emailService;
    }

    // GET /Checkout
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var cart = await _cartService.GetCartAsync(ct);
        if (cart.IsEmpty)
        {
            TempData["Error"] = "Your cart is empty.";
            return RedirectToAction("Index", "Cart");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        var dto = new CheckoutDto
        {
            RecipientName  = user.FullName,
            RecipientPhone = user.PhoneNumber ?? string.Empty,
            AddressLine    = user.AddressLine ?? string.Empty,
            Ward           = user.Ward        ?? string.Empty,
            District       = user.District    ?? string.Empty,
            Province       = user.Province    ?? string.Empty,
            Cart           = cart,
        };

        return View(dto);
    }

    // POST /Checkout/PlaceOrder
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutDto dto, CancellationToken ct)
    {
        var cart = await _cartService.GetCartAsync(ct);
        dto.Cart = cart;

        if (cart.IsEmpty)
            return RedirectToAction("Index", "Cart");

        if (!ModelState.IsValid)
            return View("Index", dto);

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        var shippingFee = await _cartService.CalculateShippingAsync(dto.Province, ct);

        var createDto = new CreateOrderDto
        {
            UserId         = user.Id,
            RecipientName  = dto.RecipientName,
            RecipientPhone = dto.RecipientPhone,
            AddressLine    = dto.AddressLine,
            Ward           = dto.Ward,
            District       = dto.District,
            Province       = dto.Province,
            PaymentMethod  = Enum.TryParse<PaymentMethod>(dto.PaymentMethod, out var pm)
                             ? pm : PaymentMethod.COD,
            CouponCode     = cart.CouponCode,
            ShippingFee    = shippingFee,
            Note           = dto.Note,
            Items          = cart.Items.Select(i => new CreateOrderItemDto
            {
                ProductId        = i.ProductId,
                ProductVariantId = i.ProductVariantId,
                Quantity         = i.Quantity,
            }).ToList(),
        };

        var result = await _orderService.CreateAsync(createDto, ct);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View("Index", dto);
        }

        await _cartService.ClearCartAsync(ct);

        // Email confirmation — swallow errors
        try
        {
            await _emailService.SendOrderConfirmationAsync(
                user.Email!, user.FullName,
                result.Data!.OrderCode, result.Data.Total);
        }
        catch { /* non-blocking */ }

        return RedirectToAction(nameof(Confirmation), new { id = result.Data!.Id });
    }

    // GET /Checkout/Confirmation/{id}
    public async Task<IActionResult> Confirmation(int id, CancellationToken ct)
    {
        var order = await _orderService.GetByIdAsync(id, ct);
        if (order is null) return NotFound();

        // Ensure user can only see own orders
        var user = await _userManager.GetUserAsync(User);
        if (order.UserId != user?.Id) return Forbid();

        return View(order);
    }
}