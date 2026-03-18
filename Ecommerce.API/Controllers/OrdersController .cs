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

// ═══════════════════════════════════════════════════════════════
// CUSTOMERS
// ═══════════════════════════════════════════════════════════════
[Authorize(Policy = "RequireAdmin")]
public class CustomersController : BaseApiController
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
        => _customerService = customerService;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] CustomerFilterParams filter, CancellationToken ct)
    {
        var result = await _customerService.GetPagedAsync(filter, ct);
        return Paged(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var customer = await _customerService.GetByIdAsync(id, ct);
        return customer is null ? NotFound("Customer not found.") : Ok(customer);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        string id, [FromBody] UpdateCustomerDto dto, CancellationToken ct)
    {
        dto.Id = id;
        var result = await _customerService.UpdateAsync(dto, ct);
        return FromResult(result, "Customer updated.");
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(string id, CancellationToken ct)
    {
        var result = await _customerService.SetActiveAsync(id, isActive: true, ct);
        return FromResult(result, "Customer activated.");
    }

    [HttpPost("{id}/block")]
    public async Task<IActionResult> Block(string id, CancellationToken ct)
    {
        var result = await _customerService.SetActiveAsync(id, isActive: false, ct);
        return FromResult(result, "Customer blocked.");
    }
}

// ═══════════════════════════════════════════════════════════════
// USERS MANAGER
// ═══════════════════════════════════════════════════════════════
[Authorize(Policy = "RequireAdmin")]
public class UsersController : BaseApiController
{
    private readonly IUsersManagerService _usersService;

    public UsersController(IUsersManagerService usersService)
        => _usersService = usersService;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] UserManagerFilterParams filter, CancellationToken ct)
    {
        var result = await _usersService.GetPagedAsync(filter, ct);
        return Paged(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var user = await _usersService.GetByIdAsync(id, ct);
        return user is null ? NotFound("User not found.") : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] AdminCreateUserDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest("Validation failed.");
        var result = await _usersService.CreateAsync(dto, ct);
        return FromResult(result, "User created.");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        string id, [FromBody] AdminEditUserDto dto, CancellationToken ct)
    {
        dto.Id = id;
        var result = await _usersService.UpdateAsync(dto, ct);
        return FromResult(result, "User updated.");
    }

    [HttpPost("{id}/lock")]
    public async Task<IActionResult> Lock(string id, CancellationToken ct)
    {
        var result = await _usersService.LockAsync(id, ct);
        return FromResult(result, "User locked.");
    }

    [HttpPost("{id}/unlock")]
    public async Task<IActionResult> Unlock(string id, CancellationToken ct)
    {
        var result = await _usersService.UnlockAsync(id, ct);
        return FromResult(result, "User unlocked.");
    }

    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(
        string id, [FromBody] AdminResetPasswordDto dto, CancellationToken ct)
    {
        dto.Id = id;
        var result = await _usersService.ResetPasswordAsync(dto, ct);
        return FromResult(result, "Password reset.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var result = await _usersService.DeleteAsync(id, ct);
        return FromResult(result, "User deleted.");
    }
}

// ═══════════════════════════════════════════════════════════════
// ROLES
// ═══════════════════════════════════════════════════════════════
[Authorize(Policy = "RequireAdmin")]
public class RolesController : BaseApiController
{
    private readonly IRolesService _rolesService;

    public RolesController(IRolesService rolesService)
        => _rolesService = rolesService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var roles = await _rolesService.GetAllAsync(ct);
        return Ok(roles);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateRoleDto dto, CancellationToken ct)
    {
        var result = await _rolesService.CreateAsync(dto, ct);
        return FromResult(result, "Role created.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var result = await _rolesService.DeleteAsync(id, ct);
        return FromResult(result, "Role deleted.");
    }

    [HttpPost("assign")]
    public async Task<IActionResult> Assign(
        [FromBody] AssignRoleDto dto, CancellationToken ct)
    {
        var result = await _rolesService.AssignRoleAsync(dto, ct);
        return FromResult(result, "Role assigned.");
    }

    [HttpPost("remove")]
    public async Task<IActionResult> Remove(
        [FromBody] AssignRoleDto dto, CancellationToken ct)
    {
        var result = await _rolesService.RemoveRoleAsync(dto, ct);
        return FromResult(result, "Role removed.");
    }
}