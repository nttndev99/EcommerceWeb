using Microsoft.AspNetCore.Mvc;
using Ecommerce.Application.Interfaces;

namespace Ecommerce.MVC.ViewComponents;

public class CartBadgeViewComponent : ViewComponent
{
    private readonly ICartService _cartService;

    public CartBadgeViewComponent(ICartService cartService)
        => _cartService = cartService;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var cart = await _cartService.GetCartAsync();
        ViewBag.CartSubTotal = cart.SubTotal > 0
            ? $"{cart.SubTotal:N0} ₫"
            : "$0.00";
        return View(cart.TotalItems);
    }





}