using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Customer;
using Ecommerce.Application.DTOs.Order;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _uow;
    private readonly IIdentityService    _identity;

    public CustomerService(IUnitOfWork uow, IIdentityService identity)
    {
        _uow      = uow;
        _identity = identity;
    }

    // ─────────────────────────────────────────────
    // GET PAGED
    // ─────────────────────────────────────────────
    public async Task<PagedResult<CustomerListDto>> GetPagedAsync(
        CustomerFilterParams filter, CancellationToken ct = default)
    {
        var customerUserIds = await _uow.Customers.GetUserIdsByRoleAsync("Customer", ct);
        var (users, total)  = await _uow.Customers.GetPagedAsync(filter, customerUserIds, ct);

        var userIds    = users.Select(u => u.Id).ToList();
        var statsAll   = await _uow.Customers.GetOrderStatsAsync(userIds, ct);
        var statsMap   = statsAll.ToDictionary(s => s.UserId);

        var items = users.Select(u =>
        {
            statsMap.TryGetValue(u.Id, out var stats);
            return new CustomerListDto
            {
                Id          = u.Id,
                FullName    = u.FullName,
                Email       = u.Email ?? string.Empty,
                PhoneNumber = u.PhoneNumber,
                AvatarUrl   = u.AvatarUrl,
                IsActive    = u.IsActive,
                CreatedAt   = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                TotalOrders = stats?.TotalOrders ?? 0,
                TotalSpent  = stats?.TotalSpent  ?? 0,
            };
        }).ToList();

        return PagedResult<CustomerListDto>.Create(items, total, filter.PageNumber, filter.PageSize);
    }

    // ─────────────────────────────────────────────
    // GET BY ID
    // ─────────────────────────────────────────────
    public async Task<CustomerDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var user = await _uow.Customers.GetByIdAsync(id, ct);
        if (user is null) return null;

        var stats = await _uow.Customers.GetOrderStatByUserAsync(id, ct);

        return new CustomerDto
        {
            Id          = user.Id,
            FullName    = user.FullName,
            Email       = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl   = user.AvatarUrl,
            Gender      = user.Gender,
            DateOfBirth = user.DateOfBirth,
            AddressLine = user.AddressLine,
            Ward        = user.Ward,
            District    = user.District,
            Province    = user.Province,
            IsActive    = user.IsActive,
            CreatedAt   = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            TotalOrders = stats?.TotalOrders ?? 0,
            TotalSpent  = stats?.TotalSpent  ?? 0,
        };
    }

    // ─────────────────────────────────────────────
    // GET ORDERS
    // ─────────────────────────────────────────────
    public async Task<PagedResult<OrderListDto>> GetOrdersAsync(
        string userId, PaginationParams pagination, CancellationToken ct = default)
    {
        var (orders, total) = await _uow.Customers.GetOrdersByUserAsync(userId, pagination, ct);

        var items = orders.Select(o => new OrderListDto
        {
            Id            = o.Id,
            OrderCode     = o.OrderCode,
            UserId        = o.UserId,
            OrderStatus   = o.OrderStatus,
            PaymentStatus = o.PaymentStatus,
            PaymentMethod = o.PaymentMethod,
            RecipientName = o.RecipientName,
            Total         = o.Total,
            ItemCount     = o.Items.Count,
            CreatedAt     = o.CreatedAt,
        }).ToList();

        return PagedResult<OrderListDto>.Create(items, total, pagination.PageNumber, pagination.PageSize);
    }

    // ─────────────────────────────────────────────
    // UPDATE
    // ─────────────────────────────────────────────
    public async Task<Result<CustomerDto>> UpdateAsync(
        UpdateCustomerDto dto, CancellationToken ct = default)
    {
        var user = await _identity.FindByIdAsync(dto.Id);
        if (user is null)
            return Result<CustomerDto>.Failure($"Customer #{dto.Id} not found.");

        user.FullName    = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;
        user.Gender      = dto.Gender;
        user.DateOfBirth = dto.DateOfBirth;
        user.AddressLine = dto.AddressLine;
        user.Ward        = dto.Ward;
        user.District    = dto.District;
        user.Province    = dto.Province;
        user.IsActive    = dto.IsActive;

        var result = await _identity.UpdateAsync(user);
        if (!result.IsSuccess)
            return Result<CustomerDto>.Failure(result.Error!);

        return Result<CustomerDto>.Success((await GetByIdAsync(user.Id, ct))!);
    }

    // ─────────────────────────────────────────────
    // SET ACTIVE
    // ─────────────────────────────────────────────
    public async Task<Result> SetActiveAsync(string id, bool isActive, CancellationToken ct = default)
    {
        var user = await _identity.FindByIdAsync(id);
        if (user is null)
            return Result.Failure($"Customer #{id} not found.");

        user.IsActive = isActive;
        var updateResult = await _identity.UpdateAsync(user);
        if (!updateResult.IsSuccess)
            return Result.Failure(updateResult.Error!);

        return await _identity.SetLockoutAsync(id, !isActive);
    }

    // ─────────────────────────────────────────────
    // COUNT
    // ─────────────────────────────────────────────
    public async Task<int> CountAsync(bool? isActive = null, CancellationToken ct = default)
    {
        var customerUserIds = await _uow.Customers.GetUserIdsByRoleAsync("Customer", ct);
        return await _uow.Customers.CountAsync(customerUserIds, isActive, ct);
    }
}