using Microsoft.AspNetCore.Mvc;
using Ecommerce.Application.DTOs.Cart;
using Ecommerce.Application.Interfaces;

namespace Ecommerce.MVC.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
        => _cartService = cartService;

    // GET /Cart
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var cart = await _cartService.GetCartAsync(ct);
        return View(cart);
    }

    // POST /Cart/Add
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(AddToCartDto dto, string? returnUrl, CancellationToken ct)
    {
        var result = await _cartService.AddToCartAsync(dto, ct);

        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "Item added to cart."
            : result.Error;

        return RedirectToLocal(returnUrl);
    }

    // POST /Cart/AddAndCheckout  ← Buy Now
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAndCheckout(AddToCartDto dto, CancellationToken ct)
    {
        await _cartService.AddToCartAsync(dto, ct);
        return RedirectToAction("Index", "Checkout");
    }

    // POST /Cart/UpdateQuantity
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(UpdateCartItemDto dto, CancellationToken ct)
    {
        var result = await _cartService.UpdateQuantityAsync(dto, ct);

        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "Cart updated."
            : result.Error;

        return RedirectToAction(nameof(Index));
    }

    // POST /Cart/Remove
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(string cartItemId, CancellationToken ct)
    {
        await _cartService.RemoveItemAsync(cartItemId, ct);
        TempData["Success"] = "Item removed.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Cart/ApplyCoupon
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyCoupon(ApplyCouponDto dto, CancellationToken ct)
    {
        var result = await _cartService.ApplyCouponAsync(dto.Code, ct);

        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? $"Coupon '{dto.Code}' applied!"
            : result.Error;

        return RedirectToAction(nameof(Index));
    }

    // POST /Cart/RemoveCoupon
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveCoupon(CancellationToken ct)
    {
        await _cartService.RemoveCouponAsync(ct);
        TempData["Success"] = "Coupon removed.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Cart/UpdateShipping
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateShipping(string province, CancellationToken ct)
    {
        var fee  = await _cartService.CalculateShippingAsync(province, ct);
        var cart = await _cartService.GetCartAsync(ct);
        cart.ShippingFee = fee;
        TempData["ShippingFee"]      = fee.ToString("N0");
        TempData["ShippingProvince"] = province;
        return RedirectToAction(nameof(Index));
    }

    private IActionResult RedirectToLocal(string? url)
        => !string.IsNullOrEmpty(url) && Url.IsLocalUrl(url)
            ? Redirect(url)
            : RedirectToAction(nameof(Index));
}