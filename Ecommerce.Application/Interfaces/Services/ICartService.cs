using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Cart;

namespace Ecommerce.Application.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(CancellationToken ct = default);
    Task<Result> AddToCartAsync(AddToCartDto dto, CancellationToken ct = default);
    Task<Result> UpdateQuantityAsync(UpdateCartItemDto dto, CancellationToken ct = default);
    Task<Result> RemoveItemAsync(string cartItemId, CancellationToken ct = default);
    Task ClearCartAsync(CancellationToken ct = default);
    Task<Result> ApplyCouponAsync(string code, CancellationToken ct = default);
    Task RemoveCouponAsync(CancellationToken ct = default);
    Task<decimal> CalculateShippingAsync(string province, CancellationToken ct = default);
    Task MergeSessionCartAsync(string userId, CancellationToken ct = default);
}