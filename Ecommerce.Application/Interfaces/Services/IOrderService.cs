using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Order;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.Interfaces;

public interface IOrderService
{
    Task<PagedResult<OrderListDto>> GetPagedAsync(OrderFilterParams filter, CancellationToken ct = default);
    Task<OrderDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<OrderDto?> GetByCodeAsync(string orderCode, CancellationToken ct = default);
    Task<Result<OrderDto>> CreateAsync(CreateOrderDto dto, CancellationToken ct = default);
    Task<Result<OrderDto>> UpdateOrderStatusAsync(UpdateOrderStatusDto dto, CancellationToken ct = default);
    Task<Result<OrderDto>> UpdatePaymentStatusAsync(UpdatePaymentStatusDto dto, CancellationToken ct = default);
    Task<Result<OrderDto>> CancelAsync(int id, string reason, CancellationToken ct = default);
    Task<int> CountByStatusAsync(OrderStatus? status = null, CancellationToken ct = default);
}