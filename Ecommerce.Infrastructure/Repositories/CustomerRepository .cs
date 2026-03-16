using Microsoft.EntityFrameworkCore;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Customer;
using Ecommerce.Application.DTOs.Order;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Infrastructure.Persistence;
using Ecommerce.Application.Interfaces.Repositories;

namespace Ecommerce.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly EcommerceDbContext _context;

    public CustomerRepository(EcommerceDbContext context)
    {
        _context = context;
    }

    // ─────────────────────────────────────────────
    // GET USER IDS BY ROLE
    // ─────────────────────────────────────────────
    public async Task<IEnumerable<string>> GetUserIdsByRoleAsync(
        string roleName, CancellationToken ct = default)
    {
        var role = await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == roleName, ct);

        if (role is null) return Enumerable.Empty<string>();

        return await _context.UserRoles
            .AsNoTracking()
            .Where(ur => ur.RoleId == role.Id)
            .Select(ur => ur.UserId)
            .ToListAsync(ct);
    }

    // ─────────────────────────────────────────────
    // GET PAGED
    // ─────────────────────────────────────────────
    public async Task<(IEnumerable<AppUser> Items, int TotalCount)> GetPagedAsync(
        CustomerFilterParams filter,
        IEnumerable<string> customerUserIds,
        CancellationToken ct = default)
    {
        var ids   = customerUserIds.ToHashSet();
        var query = _context.Users
            .AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var kw = filter.Search.Trim().ToLower();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(kw) ||
                (u.Email        != null && u.Email.ToLower().Contains(kw)) ||
                (u.PhoneNumber  != null && u.PhoneNumber.Contains(kw)));
        }

        if (filter.IsActive.HasValue)
            query = query.Where(u => u.IsActive == filter.IsActive.Value);

        query = (filter.SortBy?.ToLower(), filter.SortDirection?.ToLower()) switch
        {
            ("fullname",  "asc") => query.OrderBy(u => u.FullName),
            ("fullname",  _)     => query.OrderByDescending(u => u.FullName),
            ("lastlogin", "asc") => query.OrderBy(u => u.LastLoginAt),
            ("lastlogin", _)     => query.OrderByDescending(u => u.LastLoginAt),
            ("createdat", "asc") => query.OrderBy(u => u.CreatedAt),
            _                    => query.OrderByDescending(u => u.CreatedAt),
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    // ─────────────────────────────────────────────
    // GET BY ID
    // ─────────────────────────────────────────────
    public async Task<AppUser?> GetByIdAsync(string id, CancellationToken ct = default)
        => await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    // ─────────────────────────────────────────────
    // ORDER STATS (many users)
    // ─────────────────────────────────────────────
    public async Task<IEnumerable<OrderStatDto>> GetOrderStatsAsync(
        IEnumerable<string> userIds, CancellationToken ct = default)
    {
        var ids = userIds.ToList();
        return await _context.Orders
            .AsNoTracking()
            .Where(o => ids.Contains(o.UserId))
            .GroupBy(o => o.UserId)
            .Select(g => new OrderStatDto
            {
                UserId      = g.Key,
                TotalOrders = g.Count(),
                TotalSpent  = g.Where(o => o.OrderStatus == OrderStatus.Delivered)
                               .Sum(o => (decimal?)o.Total) ?? 0,
            })
            .ToListAsync(ct);
    }

    // ─────────────────────────────────────────────
    // ORDER STAT (single user)
    // ─────────────────────────────────────────────
    public async Task<OrderStatDto?> GetOrderStatByUserAsync(
        string userId, CancellationToken ct = default)
        => await _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .GroupBy(o => o.UserId)
            .Select(g => new OrderStatDto
            {
                UserId      = g.Key,
                TotalOrders = g.Count(),
                TotalSpent  = g.Where(o => o.OrderStatus == OrderStatus.Delivered)
                               .Sum(o => (decimal?)o.Total) ?? 0,
            })
            .FirstOrDefaultAsync(ct);

    // ─────────────────────────────────────────────
    // ORDERS BY USER
    // ─────────────────────────────────────────────
    public async Task<(IEnumerable<Order> Items, int TotalCount)> GetOrdersByUserAsync(
        string userId, PaginationParams pagination, CancellationToken ct = default)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt);

        var total  = await query.CountAsync(ct);
        var orders = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(ct);

        return (orders, total);
    }

    // ─────────────────────────────────────────────
    // COUNT
    // ─────────────────────────────────────────────
    public async Task<int> CountAsync(
        IEnumerable<string> customerUserIds, bool? isActive, CancellationToken ct = default)
    {
        var ids   = customerUserIds.ToHashSet();
        var query = _context.Users
            .AsNoTracking()
            .Where(u => ids.Contains(u.Id));

        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        return await query.CountAsync(ct);
    }
}