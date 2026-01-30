using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    private IRepository<Listing>? _listings;
    private IRepository<Order>? _orders;
    private IRepository<OrderItem>? _orderItems;
    private IRepository<Category>? _categories;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IRepository<Listing> Listings
    {
        get
        {
            return _listings ??= new Repository<Listing>(_context);
        }
    }

    public IRepository<Order> Orders
    {
        get
        {
            return _orders ??= new Repository<Order>(_context);
        }
    }

    public IRepository<OrderItem> OrderItems
    {
        get
        {
            return _orderItems ??= new Repository<OrderItem>(_context);
        }
    }

    public IRepository<Category> Categories
    {
        get
        {
            return _categories ??= new Repository<Category>(_context);
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        if (_context.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            await _context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");
        }
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
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
