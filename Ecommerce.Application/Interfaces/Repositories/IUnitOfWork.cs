using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Application.Repositories.Interfaces;
using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ICategoryRepository Categories { get; }
    IProductRepository Products { get; }
    IProductVariantRepository ProductVariants { get; }
    IProductImageRepository ProductImages { get; }
    IInventoryRepository Inventories { get; }
    IOrderRepository Orders { get; }
    ICustomerRepository Customers { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
