using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ecommerce.Application.Interfaces;

namespace Ecommerce.MVC.Filters;

public class CartLayoutFilter : IAsyncActionFilter
{
    private readonly ICartService _cartService;

    public CartLayoutFilter(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var result = await next();

        if (context.Controller is Controller controller)
        {
            try
            {
                var cart = await _cartService.GetCartAsync();
                controller.ViewBag.CartItemCount = cart.TotalItems;
            }
            catch { /* swallow — don't break page if cart fails */ }
        }
    }
}

// ─────────────────────────────────────────────
// EmailService addition — SendOrderConfirmationAsync
// Add this method to your existing EmailService.cs
// ─────────────────────────────────────────────
/*
public async Task SendOrderConfirmationAsync(
    string toEmail, string fullName, string orderCode, decimal total)
{
    var body = $"""
        <h2>Order Confirmed ✓</h2>
        <p>Hi <strong>{fullName}</strong>,</p>
        <p>Thank you for your order! Here's your summary:</p>
        <table style="border-collapse:collapse;width:100%;max-width:480px">
            <tr>
                <td style="padding:8px;border:1px solid #e5e7eb">Order Code</td>
                <td style="padding:8px;border:1px solid #e5e7eb"><strong>{orderCode}</strong></td>
            </tr>
            <tr>
                <td style="padding:8px;border:1px solid #e5e7eb">Total</td>
                <td style="padding:8px;border:1px solid #e5e7eb"><strong>{total:N0} ₫</strong></td>
            </tr>
        </table>
        <p style="margin-top:1.5rem">
            <a href="{BaseUrl}/Orders/Detail"
               style="display:inline-block;padding:10px 24px;background:#111;color:#fff;
                      border-radius:6px;text-decoration:none;font-weight:600">
                View Order
            </a>
        </p>
        <p style="color:#888;font-size:.85rem">
            If you have any questions, reply to this email.
        </p>
        """;

    await SendAsync(toEmail, $"Order Confirmed — {orderCode}", body);
}
*/