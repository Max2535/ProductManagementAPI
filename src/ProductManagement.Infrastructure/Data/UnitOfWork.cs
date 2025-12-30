using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Interfaces;
using ProductManagement.Infrastructure.Repositories;

namespace ProductManagement.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private IDbContextTransaction? _transaction;

    // Lazy initialization
    private IProductRepository? _products;
    private IRepository<Category, Guid>? _categories;
    private IRepository<Review, Guid>? _reviews;
    private IUserRepository? _users;
    private IRepository<Role, Guid>? _roles;
    private IOrderRepository? _orders;

    public UnitOfWork(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public IProductRepository Products =>
        _products ??= new ProductRepository(_context, _configuration);

    public IRepository<Category, Guid> Categories =>
        _categories ??= new Repository<Category, Guid>(_context);

    public IRepository<Review, Guid> Reviews =>
        _reviews ??= new Repository<Review, Guid>(_context);

    public IUserRepository Users =>
        _users ??= new UserRepository(_context);

    public IRepository<Role, Guid> Roles =>
        _roles ??= new Repository<Role, Guid>(_context);

    public IOrderRepository Orders =>
        _orders ??= new OrderRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await (_transaction?.CommitAsync(cancellationToken) ?? Task.CompletedTask);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await (_transaction?.RollbackAsync(cancellationToken) ?? Task.CompletedTask);
        _transaction?.Dispose();
        _transaction = null;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}