using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Customer;
using Ecommerce.Application.DTOs.Order;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface ICustomerRepository
{
    Task<(IEnumerable<AppUser> Items, int TotalCount)> GetPagedAsync(
        CustomerFilterParams filter, IEnumerable<string> customerUserIds, CancellationToken ct = default);

    Task<AppUser?> GetByIdAsync(string id, CancellationToken ct = default);

    Task<IEnumerable<string>> GetUserIdsByRoleAsync(string roleName, CancellationToken ct = default);

    Task<IEnumerable<OrderStatDto>> GetOrderStatsAsync(
        IEnumerable<string> userIds, CancellationToken ct = default);

    Task<OrderStatDto?> GetOrderStatByUserAsync(string userId, CancellationToken ct = default);

    Task<(IEnumerable<Order> Items, int TotalCount)> GetOrdersByUserAsync(
        string userId, PaginationParams pagination, CancellationToken ct = default);

    Task<int> CountAsync(IEnumerable<string> customerUserIds, bool? isActive, CancellationToken ct = default);
}

