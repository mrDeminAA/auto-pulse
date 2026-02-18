using AutoPulse.Domain;

namespace AutoPulse.Application.Common.Interfaces;

/// <summary>
/// Unit of Work - управление транзакциями и репозиториями
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IRepository<Domain.Market> Markets { get; }
    IRepository<Domain.Dealer> Dealers { get; }
    IRepository<Domain.Car> Cars { get; }
    IRepository<Domain.Brand> Brands { get; }
    IRepository<Domain.Model> Models { get; }
    IRepository<Domain.DataSource> DataSources { get; }
    
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
