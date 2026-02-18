using AutoPulse.Application.Common.Interfaces;
using AutoPulse.Application.Markets.DTOs;
using MediatR;

namespace AutoPulse.Application.Markets.Queries;

/// <summary>
/// Query: Получить количество автомобилей на рынке
/// </summary>
public record GetMarketCarsCountQuery(int MarketId) : IRequest<CarsCountDto?>;

/// <summary>
/// Handler для GetMarketCarsCountQuery
/// </summary>
public class GetMarketCarsCountHandler : IRequestHandler<GetMarketCarsCountQuery, CarsCountDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMarketCarsCountHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CarsCountDto?> Handle(GetMarketCarsCountQuery request, CancellationToken ct)
    {
        // Проверяем существование рынка
        var marketExists = await _unitOfWork.Markets.ExistsAsync(m => m.Id == request.MarketId, ct);
        if (!marketExists) return null;

        var totalCount = await _unitOfWork.Cars.CountAsync(c => c.MarketId == request.MarketId, ct);
        var availableCount = await _unitOfWork.Cars.CountAsync(c => c.MarketId == request.MarketId && c.IsAvailable, ct);

        return new CarsCountDto(totalCount, availableCount);
    }
}
