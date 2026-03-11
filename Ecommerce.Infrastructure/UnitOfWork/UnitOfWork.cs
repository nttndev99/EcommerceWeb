
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Application.Repositories.Interfaces;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Persistence;
using Ecommerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ecommerce.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly EcommerceDbContext _context;
    private IDbContextTransaction? _transaction;

    private ICategoryRepository? _categories;
    private IProductRepository? _products;
    private IProductVariantRepository? _productVariants;
    private IProductImageRepository? _productImages;
    private IInventoryRepository? _inventories;

    public UnitOfWork(EcommerceDbContext context) => _context = context;

    public ICategoryRepository Categories
        => _categories ??= new CategoryRepository(_context);

    public IProductRepository Products
        => _products ??= new ProductRepository(_context);

    public IProductVariantRepository ProductVariants
        => _productVariants ??= new ProductVariantRepository(_context);

    public IProductImageRepository ProductImages
        => _productImages ??= new ProductImageRepository(_context);

    public IInventoryRepository Inventories
        => _inventories ??= new InventoryRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await _context.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
