using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Application.DTOs.Order;
using Ecommerce.Application.Repositories.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.Interfaces.Repositories
{
    public interface IOrderRepository 
    {
        Task<Order?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Order?> GetByCodeAsync(string orderCode, CancellationToken ct = default);
        Task<(IEnumerable<Order> Items, int TotalCount)> GetPagedAsync(OrderFilterParams filter, CancellationToken ct = default);
        Task<int> CountByStatusAsync(OrderStatus? status, CancellationToken ct = default);
        Task<int> CountByPaymentStatusAsync(PaymentStatus? status, CancellationToken ct = default);
        Task AddAsync(Order order, CancellationToken ct = default);
        Task UpdateAsync(Order order, CancellationToken ct = default);
        Task<string> GenerateOrderCodeAsync(CancellationToken ct = default);
    }
}