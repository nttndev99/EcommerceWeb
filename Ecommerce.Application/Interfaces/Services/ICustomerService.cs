using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Customer;
using Ecommerce.Application.DTOs.Order;

namespace Ecommerce.Application.Interfaces;

public interface ICustomerService
{
    Task<PagedResult<CustomerListDto>> GetPagedAsync(CustomerFilterParams filter, CancellationToken ct = default);
    Task<CustomerDto?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<PagedResult<OrderListDto>> GetOrdersAsync(string userId, PaginationParams pagination, CancellationToken ct = default);
    Task<Result<CustomerDto>> UpdateAsync(UpdateCustomerDto dto, CancellationToken ct = default);
    Task<Result> SetActiveAsync(string id, bool isActive, CancellationToken ct = default);
    Task<int> CountAsync(bool? isActive = null, CancellationToken ct = default);
}