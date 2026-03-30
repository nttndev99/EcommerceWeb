using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Application.DTOs.Customer;
using Ecommerce.Application.DTOs.Order;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.Domain.Enums;

namespace Ecommerce.API.Controllers;

// ═══════════════════════════════════════════════════════════════
// ORDERS
// ═══════════════════════════════════════════════════════════════
[Authorize]
public class OrdersController : BaseApiController
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
        => _orderService = orderService;

    /// <summary>Get paged orders [Admin: all, Customer: own]</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] OrderFilterParams filter, CancellationToken ct)
    {
        // Customer sees only their own orders
        if (!User.IsInRole("Admin"))
            filter.UserId = CurrentUserId;

        var result = await _orderService.GetPagedAsync(filter, ct);
        return Paged(result);
    }

    /// <summary>Get order by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var order = await _orderService.GetByIdAsync(id, ct);
        if (order is null) return NotFound($"Order {id} not found.");

        // Customer can only view own orders
        if (!User.IsInRole("Admin") && order.UserId != CurrentUserId)
            return Forbidden("You can only view your own orders.");

        return Ok(order);
    }

    /// <summary>Get order by code</summary>
    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(string code, CancellationToken ct)
    {
        var order = await _orderService.GetByCodeAsync(code, ct);
        if (order is null) return NotFound($"Order '{code}' not found.");

        if (!User.IsInRole("Admin") && order.UserId != CurrentUserId)
            return Forbidden();

        return Ok(order);
    }

    /// <summary>Create order</summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest("Validation failed.");

        // Force userId from token
        dto.UserId = CurrentUserId!;

        var result = await _orderService.CreateAsync(dto, ct);
        return result.IsSuccess
            ? Created($"/api/v1/orders/{result.Data!.Id}", result.Data, "Order placed.")
            : BadRequest(result.Error!);
    }

    /// <summary>Update order status [Admin]</summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> UpdateStatus(
        int id, [FromBody] UpdateOrderStatusDto dto, CancellationToken ct)
    {
        dto.Id = id;
        var result = await _orderService.UpdateOrderStatusAsync(dto, ct);
        return FromResult(result, "Order status updated.");
    }

    /// <summary>Update payment status [Admin]</summary>
    [HttpPatch("{id:int}/payment")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> UpdatePayment(
        int id, [FromBody] UpdatePaymentStatusDto dto, CancellationToken ct)
    {
        dto.Id = id;
        var result = await _orderService.UpdatePaymentStatusAsync(dto, ct);
        return FromResult(result, "Payment status updated.");
    }

    /// <summary>Cancel order</summary>
    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(
        int id, [FromBody] string reason, CancellationToken ct)
    {
        var order = await _orderService.GetByIdAsync(id, ct);
        if (order is null) return NotFound("Order not found.");

        if (!User.IsInRole("Admin") && order.UserId != CurrentUserId)
            return Forbidden();

        if (!User.IsInRole("Admin") &&
            order.OrderStatus != OrderStatus.Pending &&
            order.OrderStatus != OrderStatus.Processing)
            return BadRequest("You can only cancel Pending or Processing orders.");

        var result = await _orderService.CancelAsync(id, reason, ct);
        return FromResult(result, "Order cancelled.");
    }

    /// <summary>Order statistics [Admin]</summary>
    [HttpGet("stats")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Stats(CancellationToken ct)
    {
        var stats = new
        {
            total     = await _orderService.CountByStatusAsync(null, ct),
            pending   = await _orderService.CountByStatusAsync(OrderStatus.Pending, ct),
            processing= await _orderService.CountByStatusAsync(OrderStatus.Processing, ct),
            shipped   = await _orderService.CountByStatusAsync(OrderStatus.Shipped, ct),
            delivered = await _orderService.CountByStatusAsync(OrderStatus.Delivered, ct),
            cancelled = await _orderService.CountByStatusAsync(OrderStatus.Cancelled, ct),
        };
        return Ok(stats);
    }
}





