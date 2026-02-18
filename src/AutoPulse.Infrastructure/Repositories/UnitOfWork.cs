using AutoPulse.Application.Common.Interfaces;
using AutoPulse.Domain;
using AutoPulse.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace AutoPulse.Infrastructure.Repositories;

/// <summary>
/// Unit of Work - управление транзакциями и репозиториями
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    private IRepository<Market>? _markets;
    private IRepository<Dealer>? _dealers;
    private IRepository<Car>? _cars;
    private IRepository<Brand>? _brands;
    private IRepository<Model>? _models;
    private IRepository<DataSource>? _dataSources;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IRepository<Market> Markets =>
        _markets ??= new Repository<Market>(_context);

    public IRepository<Dealer> Dealers =>
        _dealers ??= new Repository<Dealer>(_context);

    public IRepository<Car> Cars =>
        _cars ??= new Repository<Car>(_context);

    public IRepository<Brand> Brands =>
        _brands ??= new Repository<Brand>(_context);

    public IRepository<Model> Models =>
        _models ??= new Repository<Model>(_context);

    public IRepository<DataSource> DataSources =>
        _dataSources ??= new Repository<DataSource>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
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
        GC.SuppressFinalize(this);
    }
}
