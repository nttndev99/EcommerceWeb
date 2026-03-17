using Microsoft.AspNetCore.Mvc;
using Ecommerce.Application.DTOs.Order;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Ecommerce.Web.Controllers;

[Area("Admin")]
[Authorize(Policy = "RequireAdmin")]

public class OrdersController : Controller
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // ─────────────────────────────────────────────
    // GET /Orders
    // ─────────────────────────────────────────────
    public async Task<IActionResult> Index(OrderFilterParams filter, CancellationToken ct)
    {
        var result = await _orderService.GetPagedAsync(filter, ct);

        ViewBag.Filter          = filter;
        ViewBag.TotalAll        = await _orderService.CountByStatusAsync(null,                      ct);
        ViewBag.TotalPending    = await _orderService.CountByStatusAsync(OrderStatus.Pending,    ct);
        ViewBag.TotalProcessing = await _orderService.CountByStatusAsync(OrderStatus.Processing, ct);
        ViewBag.TotalShipped    = await _orderService.CountByStatusAsync(OrderStatus.Shipped,    ct);
        ViewBag.TotalDelivered  = await _orderService.CountByStatusAsync(OrderStatus.Delivered,  ct);
        ViewBag.TotalCancelled  = await _orderService.CountByStatusAsync(OrderStatus.Cancelled,  ct);

        return View(result);
    }

    // ─────────────────────────────────────────────
    // GET /Orders/Detail/{id}
    // ─────────────────────────────────────────────
    public async Task<IActionResult> Detail(int id, CancellationToken ct)
    {
        var order = await _orderService.GetByIdAsync(id, ct);
        if (order is null) return NotFound();
        return View(order);
    }

    // ─────────────────────────────────────────────
    // POST /Orders/UpdateStatus
    // ─────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(UpdateOrderStatusDto dto, CancellationToken ct)
    {
        var result = await _orderService.UpdateOrderStatusAsync(dto, ct);

        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? $"Order status updated to '{dto.OrderStatus}'."
            : result.Error;

        return RedirectToAction(nameof(Detail), new { id = dto.Id });
    }

    // ─────────────────────────────────────────────
    // POST /Orders/UpdatePayment
    // ─────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePayment(UpdatePaymentStatusDto dto, CancellationToken ct)
    {
        var result = await _orderService.UpdatePaymentStatusAsync(dto, ct);

        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? $"Payment status updated to '{dto.PaymentStatus}'."
            : result.Error;

        return RedirectToAction(nameof(Detail), new { id = dto.Id });
    }

    // ─────────────────────────────────────────────
    // POST /Orders/Cancel
    // ─────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string reason, CancellationToken ct)
    {
        var result = await _orderService.CancelAsync(id, reason, ct);

        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "Order cancelled successfully."
            : result.Error;

        return RedirectToAction(nameof(Detail), new { id });
    }
}