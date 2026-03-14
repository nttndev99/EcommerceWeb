using Microsoft.EntityFrameworkCore;
using Ecommerce.Application.DTOs.Order;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Infrastructure.Persistence;

namespace Ecommerce.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly EcommerceDbContext _context;
 
    public OrderRepository(EcommerceDbContext context)
    {
        _context = context;
    }
    public async Task<Order?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Items)
                .ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<Order?> GetByCodeAsync(string orderCode, CancellationToken ct = default)
        => await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Items)
                .ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(o => o.OrderCode == orderCode, ct);

    public async Task<(IEnumerable<Order> Items, int TotalCount)> GetPagedAsync(
        OrderFilterParams filter, CancellationToken ct = default)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .AsQueryable();

        // ── Filters ───────────────────────────────
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var kw = filter.Search.Trim().ToLower();
            query = query.Where(o =>
                o.OrderCode.ToLower().Contains(kw) ||
                o.RecipientName.ToLower().Contains(kw) ||
                o.RecipientPhone.Contains(kw));
        }

        if (!string.IsNullOrWhiteSpace(filter.UserId))
            query = query.Where(o => o.UserId == filter.UserId);

        if (filter.OrderStatus.HasValue)
            query = query.Where(o => o.OrderStatus == filter.OrderStatus.Value);

        if (filter.PaymentStatus.HasValue)
            query = query.Where(o => o.PaymentStatus == filter.PaymentStatus.Value);

        if (filter.From.HasValue)
            query = query.Where(o => o.CreatedAt >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(o => o.CreatedAt <= filter.To.Value.AddDays(1));

        // ── Sort ──────────────────────────────────
        query = (filter.SortBy?.ToLower(), filter.SortDirection?.ToLower()) switch
        {
            ("total",     "asc") => query.OrderBy(o => o.Total),
            ("total",     _)     => query.OrderByDescending(o => o.Total),
            ("createdat", "asc") => query.OrderBy(o => o.CreatedAt),
            _                    => query.OrderByDescending(o => o.CreatedAt),
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<int> CountByStatusAsync(OrderStatus? status, CancellationToken ct = default)
    {
        var query = _context.Orders.AsNoTracking();
        if (status.HasValue)
            query = query.Where(o => o.OrderStatus == status.Value);
        return await query.CountAsync(ct);
    }

    public async Task<int> CountByPaymentStatusAsync(PaymentStatus? status, CancellationToken ct = default)
    {
        var query = _context.Orders.AsNoTracking();
        if (status.HasValue)
            query = query.Where(o => o.PaymentStatus == status.Value);
        return await query.CountAsync(ct);
    }

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        await _context.Orders.AddAsync(order, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<string> GenerateOrderCodeAsync(CancellationToken ct = default)
    {
        var prefix     = $"ORD-{DateTime.UtcNow:yyyyMMdd}-";
        var todayCount = await _context.Orders
            .CountAsync(o => o.OrderCode.StartsWith(prefix), ct);
        return $"{prefix}{(todayCount + 1):D4}";
    }
}