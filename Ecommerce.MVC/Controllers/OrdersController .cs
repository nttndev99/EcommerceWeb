using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.Application.DTOs.Order;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;

namespace Ecommerce.MVC.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly IOrderService        _orderService;
    private readonly UserManager<AppUser> _userManager;

    public OrdersController(IOrderService orderService, UserManager<AppUser> userManager)
    {
        _orderService = orderService;
        _userManager  = userManager;
    }

    // GET /Orders
    public async Task<IActionResult> Index(OrderFilterParams filter, CancellationToken ct)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        // Scope to current user
        filter.UserId = user.Id;
        filter.PageSize = filter.PageSize > 0 ? filter.PageSize : 10;

        var result = await _orderService.GetPagedAsync(filter, ct);
        ViewBag.Filter = filter;
        return View(result);
    }

    // GET /Orders/Detail/{id}
    public async Task<IActionResult> Detail(int id, CancellationToken ct)
    {
        var order = await _orderService.GetByIdAsync(id, ct);
        if (order is null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (order.UserId != user?.Id) return Forbid();

        return View(order);
    }

    // POST /Orders/Cancel/{id}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string reason, CancellationToken ct)
    {
        var order = await _orderService.GetByIdAsync(id, ct);
        if (order is null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (order.UserId != user?.Id) return Forbid();

        // Only Pending/Processing can be cancelled by customer
        if (order.OrderStatus != OrderStatus.Pending && order.OrderStatus != OrderStatus.Processing)
        {
            TempData["Error"] = "This order cannot be cancelled.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        var result = await _orderService.CancelAsync(id, reason, ct);

        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "Order cancelled successfully."
            : result.Error;

        return RedirectToAction(nameof(Detail), new { id });
    }
}