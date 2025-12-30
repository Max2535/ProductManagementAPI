using ProductManagement.Domain.Entities;

namespace ProductManagement.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    IRepository<Category, Guid> Categories { get; }
    IRepository<Review, Guid> Reviews { get; }
    IUserRepository Users { get; }
    IRepository<Role, Guid> Roles { get; }
    IOrderRepository Orders { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}