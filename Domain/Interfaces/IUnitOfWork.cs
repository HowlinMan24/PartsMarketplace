using Domain.Entities;

namespace Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Listing> Listings { get; }
    IRepository<Order> Orders { get; }
    IRepository<OrderItem> OrderItems { get; }
    IRepository<Category> Categories { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
